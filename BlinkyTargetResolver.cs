using UnityEngine;

public class BlinkyTargetResolver : MonoBehaviour, IGhostTargetResolver
{
    [SerializeField] private Vector2Int wanderCell = new Vector2Int(0, 0);
    public Vector2Int ChaseTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        return context.playerCell;
    }

    public Vector2Int EscapeTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        Vector2Int away = context.thisCell - context.playerCell;
        return context.thisCell + away * 3;

    }

    public Vector2Int RespawnTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        return context.respawnCell;
    }

    public Vector2Int WanderTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        return wanderCell;
    }
}
