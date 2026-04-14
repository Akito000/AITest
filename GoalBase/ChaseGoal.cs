using UnityEngine;

/// <summary>
/// 攻撃ゴール
/// まず狙う位置を決め、そのセルへ移動する
/// </summary>
public class ChaseGoal : FinalGoal
{
    private IGhostTargetResolver resolver;

    public ChaseGoal(IGhostTargetResolver resolver)
    {
        this.resolver = resolver;
    }

    protected override void BuildSubGoals(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        Vector2Int target = resolver.ChaseTargetCell(context, memory);

        // 本当は「位置を決める」専用サブゴールを作ってもよいが、
        // 学習用としてまずは MoveToCell に落とす
        subGoals.Add(new MoveToCellSubGoal(target));
    }

    public override bool Execute(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        // Chase は毎回プレイヤー位置が変わるので、
        // 1サイクル終わるたびに作り直す
        bool finished = base.Execute(owner, context, memory);

        if (finished)
        {
            Enter(owner, context, memory);
            return false;
        }

        return false;
    }
}