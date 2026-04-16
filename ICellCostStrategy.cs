using UnityEngine;
public interface ICellCostStrategy
{
    float GetEnterCost(Vector2Int from, Vector2Int to, Stage stage, EnemyContext context);
}
