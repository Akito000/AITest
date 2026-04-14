using System.Collections.Generic;

public abstract class FinalGoal
{
    protected List<SubGoal> subGoals = new List<SubGoal>();//この目標を完了さすのに必要なサブゴール
    protected int currentSubGoalIndex = 0;//現在のサブゴール

    public virtual void Enter(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        subGoals.Clear();
        currentSubGoalIndex = 0;
        BuildSubGoals(owner, context, memory);
    }

    //この目標を完了さすのに必要なサブゴールを作る
    protected abstract void BuildSubGoals(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory);

    public virtual bool Execute(EnemyMoveAgent owner, EnemyContext context, EnemyMemory memory)
    {
        if (subGoals.Count == 0)
        {
            return true;
        }

        if (currentSubGoalIndex >= subGoals.Count)
        {
            return true;
        }

        //現在のサブゴール実行
        bool completed = subGoals[currentSubGoalIndex].Execute(owner, context, memory);

        //完了したら次のサブゴールへ
        if (completed)
        {
            currentSubGoalIndex++;
        }

        return currentSubGoalIndex >= subGoals.Count;
    }
}