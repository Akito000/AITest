/// <summary>
/// サブゴールの基底クラス
/// Execute が true を返したら、そのサブゴールは完了
/// </summary>
public abstract class SubGoal
{
    public abstract bool Execute(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory);
}