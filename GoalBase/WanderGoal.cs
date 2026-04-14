using UnityEngine;

/// <summary>
/// 巡回ポイントを順番に回る徘徊ゴール
/// 最後まで行ったらまた最初から作り直す
/// </summary>
public class WanderGoal : FinalGoal
{
    private Vector2Int[] patrolPoints;

    public WanderGoal(Vector2Int[] patrolPoints)
    {
        this.patrolPoints = patrolPoints;
    }

    //サブゴール作成
    protected override void BuildSubGoals(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        //とりあえず徘徊pointに向かうというサブゴールを格納
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            subGoals.Add(new MoveToCellSubGoal(patrolPoints[i]));
        }
    }

    public override bool Execute(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        bool finished = base.Execute(owner, context, memory);

        // 巡回は終わらないので、全部終わったら最初からやり直す
        if (finished)
        {
            Enter(owner, context, memory);
            return false;
        }

        return false;
    }
}