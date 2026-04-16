using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveAgent : MonoBehaviour, IMoveAgent
{
    private CharacterAgent agent;
    private Stage stage;

    private readonly EnemyMemory memory = new EnemyMemory();
    private IGhostTargetResolver resolver;
    private IWanderProvider wanderProvider;
    private ICellCostStrategy cellCostStrategy;

    [SerializeField] private Transform player;
    private CharacterAgent playerAgent;
    [SerializeField] private bool debugWander = false;
    [SerializeField] private bool isEscape = false;
    [SerializeField] private bool drawDebugPath = true;

    private FinalGoal currentGoalObject;

    // 現在採用している経路キャッシュ
    private readonly List<Vector2Int> currentPath = new List<Vector2Int>();
    private Vector2Int cachedPathStart;
    private Vector2Int cachedPathGoal;
    private bool hasCachedPath = false;

    private void Awake()
    {
        agent = GetComponent<CharacterAgent>();
        resolver = GetComponent<IGhostTargetResolver>();
        wanderProvider = GetComponent<IWanderProvider>();
        cellCostStrategy = GetComponent<ICellCostStrategy>();
    }

    public void Initialize(Stage stage, Transform player)
    {
        this.stage = stage;
        this.player = player;
        playerAgent = this.player.GetComponent<CharacterAgent>();
    }

    public MoveDirection NextDirection()
    {
        if (stage == null || player == null || agent == null)
        {
            return MoveDirection.NONE;
        }

        EnemyContext context = BuildContext();

        UpdateTopGoalIfNeeded(context);

        if (currentGoalObject != null)
        {
            currentGoalObject.Execute(this, context, memory);
        }

        return DecideDirection(context);
    }

    private EnemyContext BuildContext()
    {
        // Debug.Log($"{MoveDirectionUtility.ToVector2Int(playerAgent.CurrentMoveDirection)}");
        return new EnemyContext
        {
            thisCell = agent.CurrentCell,
            playerCell = stage.WorldToCell(player.position),
            playerForward = MoveDirectionUtility.ToVector2Int(playerAgent.CurrentMoveDirection),
            isCower = isEscape,
            isRespawn = false,
            isWander = debugWander,
            respawnCell = Vector2Int.zero
        };
    }

    private void UpdateTopGoalIfNeeded(EnemyContext context)
    {
        EnemyGoalType nextGoalType = ResolveTopGoalType(context);

        if (memory.currentGoal == nextGoalType && currentGoalObject != null)
        {
            return;
        }

        memory.currentGoal = nextGoalType;
        currentGoalObject = CreateGoalObject(nextGoalType);
        InvalidateCachedPath();

        if (currentGoalObject != null)
        {
            currentGoalObject.Enter(this, context, memory);
        }
    }

    private EnemyGoalType ResolveTopGoalType(EnemyContext context)
    {
        if (context.isRespawn)
        {
            return EnemyGoalType.RESPAWN;
        }

        if (context.isCower)
        {
            return EnemyGoalType.ESCAPE;
        }

        if (context.isWander)
        {
            return EnemyGoalType.WANDER;
        }

        return EnemyGoalType.ATTACK;
    }

    private FinalGoal CreateGoalObject(EnemyGoalType type)
    {
        switch (type)
        {
            case EnemyGoalType.WANDER:
                if (wanderProvider != null)
                {
                    return new WanderGoal(wanderProvider.GetWanderPoints());
                }
                break;

            case EnemyGoalType.ATTACK:
                return new ChaseGoal(resolver);
        }

        return null;
    }

    private MoveDirection DecideDirection(EnemyContext context)
    {
        Vector2Int startCell = context.thisCell;
        Vector2Int goalCell = memory.CurrentTargetCell;
        MoveDirection forbidden = GridPathfinder.Opposite(agent.CurrentMoveDirection);

        if (goalCell == startCell)
        {
            currentPath.Clear();
            currentPath.Add(startCell);
            hasCachedPath = true;
            cachedPathStart = startCell;
            cachedPathGoal = goalCell;
            return MoveDirection.NONE;
        }

        bool needsRebuild = !hasCachedPath ||
                            cachedPathStart != startCell ||
                            cachedPathGoal != goalCell ||
                            currentPath.Count < 2;

        if (needsRebuild)
        {
            RebuildPath(startCell, goalCell, context, forbidden);
        }

        if (currentPath.Count >= 2)
        {
            return ToDirection(currentPath[1] - startCell);
        }

        return GridPathfinder.FindAnyValidDirection(startCell, stage, forbidden);
    }

    private void RebuildPath(Vector2Int startCell, Vector2Int goalCell, EnemyContext context, MoveDirection forbidden)
    {
        currentPath.Clear();

        bool found = GridPathfinder.FindPathAStar(
            startCell,
            goalCell,
            stage,
            context,
            cellCostStrategy,
            forbidden,
            currentPath);

        if (!found)
        {
            currentPath.Clear();
            currentPath.Add(startCell);
        }

        hasCachedPath = true;
        cachedPathStart = startCell;
        cachedPathGoal = goalCell;
    }

    private void InvalidateCachedPath()
    {
        hasCachedPath = false;
        currentPath.Clear();
    }

    private MoveDirection ToDirection(Vector2Int delta)
    {
        if (delta == Vector2Int.up) return MoveDirection.UP;
        if (delta == Vector2Int.down) return MoveDirection.DOWN;
        if (delta == Vector2Int.left) return MoveDirection.LEFT;
        if (delta == Vector2Int.right) return MoveDirection.RIGHT;
        return MoveDirection.NONE;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugPath || stage == null || currentPath == null || currentPath.Count <= 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 a = stage.CellToWorld(currentPath[i]) + Vector3.up * 0.3f;
            Vector3 b = stage.CellToWorld(currentPath[i + 1]) + Vector3.up * 0.3f;
            Gizmos.DrawLine(a, b);
        }

        Gizmos.color = Color.green;
        Vector3 goalPos = stage.CellToWorld(cachedPathGoal) + Vector3.up * 0.35f;
        Gizmos.DrawSphere(goalPos, 0.15f);
    }

    public Stage GetStage()
    {
        return stage;
    }
}
