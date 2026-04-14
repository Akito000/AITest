using UnityEngine;

public class EnemyMoveAgent : MonoBehaviour, IMoveAgent
{
    //private CharacterAgent agent;
    //private Stage stage;

    //private EnemyMemory memory = new EnemyMemory();
    //private IGhostTargetResolver resolver;
    //private BlinkyTargetResolver blinkyResolver;

    //[SerializeField] private Transform player;

    //[Header("学習用：trueなら徘徊")]
    //[SerializeField] private bool debugWander = false;

    //[SerializeField] private float replanInterval = 0.1f;

    //[SerializeField] private bool isWander;

    //private void Awake()
    //{
    //    agent = GetComponent<CharacterAgent>();
    //    resolver = GetComponent<IGhostTargetResolver>();
    //    blinkyResolver = GetComponent<BlinkyTargetResolver>();
    //}

    //public void Initialize(Stage _stage, Transform _player)
    //{
    //    stage = _stage;
    //    player = _player;
    //}

    //public MoveDirection NextDirection()
    //{
    //    if (stage == null || player == null)
    //        return MoveDirection.NONE;

    //    EnemyContext context = BuildContext();

    //    // 徘徊中なら、巡回ポイント到着判定を先に行う
    //    UpdateWanderSubGoal(context);

    //    if (Time.time - memory.lastReplanTime > replanInterval)
    //    {
    //        EnemyPlan plan = SelectPlan(context);
    //        ApplyPlan(plan);
    //        memory.lastReplanTime = Time.time;
    //    }

    //    return DecideDirection(context);
    //}

    ////==========================
    //// Context生成
    ////==========================
    //private EnemyContext BuildContext()
    //{
    //    return new EnemyContext
    //    {
    //        thisCell = agent.CurrentCell,
    //        playerCell = stage.WorldToCell(player.position),
    //        playerForward = Vector2Int.zero,

    //        isCower = false,
    //        isRespawn = false,
    //        isWander = debugWander,

    //        respawnCell = Vector2Int.zero
    //    };
    //}

    ////==========================
    //// ゴール選択
    ////==========================
    //private EnemyPlan SelectPlan(EnemyContext context)
    //{
    //    if (context.isRespawn)
    //    {
    //        return new EnemyPlan(
    //            EnemyGoalType.RESPAWN,
    //            1000f,
    //            resolver.RespawnTargetCell(context, memory));
    //    }

    //    if (context.isCower)
    //    {
    //        return new EnemyPlan(
    //            EnemyGoalType.ESCAPE,
    //            900f,
    //            resolver.EscapeTargetCell(context, memory));
    //    }

    //    if (context.isWander)
    //    {
    //        return new EnemyPlan(
    //            EnemyGoalType.WANDER,
    //            800f,
    //            resolver.WanderTargetCell(context, memory));
    //    }

    //    return new EnemyPlan(
    //        EnemyGoalType.ATTACK,
    //        100f,
    //        resolver.ChaseTargetCell(context, memory));
    //}

    //private void ApplyPlan(EnemyPlan plan)
    //{
    //    memory.currentGoal = plan.goalType;
    //    memory.CurrentTargetCell = plan.targetCell;
    //}

    ////==========================
    //// 徘徊サブゴール更新
    ////==========================
    //private void UpdateWanderSubGoal(EnemyContext context)
    //{
    //    if (!context.isWander)
    //        return;

    //    if (blinkyResolver == null)
    //        return;

    //    // 今目指している巡回ポイントに着いたら、次の巡回ポイントへ進める
    //    Vector2Int currentPatrolTarget = resolver.WanderTargetCell(context, memory);

    //    if (context.thisCell == currentPatrolTarget)
    //    {
    //        memory.currentPatrolIndex = blinkyResolver.GetNextPatrolIndex(memory.currentPatrolIndex);
    //    }
    //}

    ////==========================
    //// 方向決定
    ////==========================
    //private MoveDirection DecideDirection(EnemyContext context)
    //{
    //    MoveDirection forbidden = GridPathfinder.Opposite(agent.CurrentMoveDirection);
    //    return GridPathfinder.FindNextDirection(
    //        context.thisCell,
    //        memory.CurrentTargetCell,
    //        stage,
    //        forbidden);
    //}

    private CharacterAgent agent;
    private Stage stage;

    private EnemyMemory memory = new EnemyMemory();
    private IGhostTargetResolver resolver;
    private IWanderProvider wanderProvider;

    [SerializeField] private Transform player;
    [SerializeField] private bool debugWander = false;

    private FinalGoal currentGoalObject;

    private void Awake()
    {
        agent = GetComponent<CharacterAgent>();
        resolver = GetComponent<IGhostTargetResolver>();
        wanderProvider = GetComponent<IWanderProvider>();
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

        UpdateTopGoalIfNeeded(context);

        if (currentGoalObject != null)
        {
            currentGoalObject.Execute(this, context, memory);
        }

        return DecideDirection(context);
    }

    private EnemyContext BuildContext()
    {
        return new EnemyContext
        {
            thisCell = agent.CurrentCell,
            playerCell = stage.WorldToCell(player.position),
            playerForward = Vector2Int.zero,

            isCower = false,
            isRespawn = false,
            isWander = debugWander,

            respawnCell = Vector2Int.zero
        };
    }

    /// <summary>
    /// 最上位ゴールが変わるべきか判定し、変わるなら作り直す
    /// </summary>
    private void UpdateTopGoalIfNeeded(EnemyContext context)
    {
        EnemyGoalType nextGoalType = ResolveTopGoalType(context);

        if (memory.currentGoal == nextGoalType && currentGoalObject != null)
        {
            return;
        }

        memory.currentGoal = nextGoalType;
        currentGoalObject = CreateGoalObject(nextGoalType, context);

        if (currentGoalObject != null)
        {
            currentGoalObject.Enter(this, context, memory);
        }
    }

    private EnemyGoalType ResolveTopGoalType(EnemyContext context)
    {
        if (context.isRespawn)
            return EnemyGoalType.RESPAWN;

        if (context.isCower)
            return EnemyGoalType.ESCAPE;

        if (context.isWander)
            return EnemyGoalType.WANDER;

        return EnemyGoalType.ATTACK;
    }

    private FinalGoal CreateGoalObject(EnemyGoalType type, EnemyContext context)
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
        MoveDirection forbidden = GridPathfinder.Opposite(agent.CurrentMoveDirection);
        return GridPathfinder.FindNextDirection(
            context.thisCell,
            memory.CurrentTargetCell,
            stage,
            forbidden);
    }

    public Stage GetStage()
    {
        return stage;
    }
}

