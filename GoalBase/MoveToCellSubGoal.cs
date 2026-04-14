using UnityEngine;

/// <summary>
/// 指定セルへ移動するサブゴール
/// 到着したら完了
/// </summary>
public class MoveToCellSubGoal : SubGoal
{
    private Vector2Int targetCell;

    public MoveToCellSubGoal(Vector2Int targetCell)
    {
        this.targetCell = targetCell;
    }

    public override bool Execute(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        // 今回の移動先を memory に反映
        memory.CurrentTargetCell = targetCell;

        // 到着したら完了
        return context.thisCell == targetCell;
    }

    public Vector2Int GetTargetCell()
    {
        return targetCell;
    }
}