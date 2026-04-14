using UnityEngine;

public class EnemyMoveAgent : MonoBehaviour, IMoveAgent
{
    private CharacterAgent agent;
    private Stage stage;

    private EnemyMemory memory = new EnemyMemory();
    private IGhostTargetResolver resolver;

    [SerializeField] private Transform player;

    private float replanInterval = 0.1f;

    private void Awake()
    {
        agent = GetComponent<CharacterAgent>();
        resolver = GetComponent<IGhostTargetResolver>();
    }
    public void Initialize(Stage _stage, Transform _player)
    {
        stage = _stage;
        player = _player;
    }
    public MoveDirection NextDirection()
    {
        if (stage == null || player == null)
            return MoveDirection.NONE;

        EnemyContext context = BuildContext();

        if (Time.time - memory.lastReplanTime > replanInterval)
        {
            var plan = SelectPlan(context);
            ApplyPlan(plan);
            memory.lastReplanTime = Time.time;
        }

        return DecideDirection(context);
    }

    //==========================
    // Context生成
    //==========================
    private EnemyContext BuildContext()
    {
        return new EnemyContext
        {
            thisCell = agent.CurrentCell,
            playerCell = stage.WorldToCell(player.position),
            playerForward = Vector2Int.zero,

            isCower = false,
            isRespawn = false,
            isWander = false,

            respawnCell = Vector2Int.zero
        };
    }

    //==========================
    // ゴール選択
    //==========================
    private EnemyPlan SelectPlan(EnemyContext context)
    {
        if (context.isRespawn)
            return new EnemyPlan(EnemyGoalType.RESPAWN, 1000f, resolver.RespawnTargetCell(context, memory));

        if (context.isCower)
            return new EnemyPlan(EnemyGoalType.ESCAPE, 900f, resolver.EscapeTargetCell(context, memory));

        return new EnemyPlan(EnemyGoalType.CHASE, 100f, resolver.ChaseTargetCell(context, memory));
    }

    private void ApplyPlan(EnemyPlan plan)
    {
        memory.currentGoal = plan.goalType;
        memory.CurrentTargetCell = plan.targetCell;
    }

    //==========================
    // 方向決定（BFS）
    //==========================
    private MoveDirection DecideDirection(EnemyContext context)
    {
        // BFS でターゲットセルへの最短経路を求め、最初の一歩を返す
        // forbidden に逆方向を渡すことでパックマンの逆走禁止ルールを実現
        MoveDirection forbidden = GridPathfinder.Opposite(agent.CurrentMoveDirection);
        return GridPathfinder.FindNextDirection(context.thisCell, memory.CurrentTargetCell, stage, forbidden);
    }
}
