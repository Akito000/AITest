using UnityEngine;

public class BlinkyTargetResolver : MonoBehaviour, IGhostTargetResolver, IWanderProvider
{
    [Header("徘徊ポイント。増やしてOK")]
    [SerializeField]
    private Vector2Int[] wanderPoints =
    {
        new Vector2Int(16, 18),
        new Vector2Int(19, 18),
        new Vector2Int(19, 20),
        new Vector2Int(16, 20),
    };

    public Vector2Int ChaseTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        return context.playerCell;
    }

    public Vector2Int EscapeTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        Vector2Int away = context.thisCell - context.playerCell;
        return context.thisCell + away * 3;
    }

    public Vector2Int[] GetWanderPoints()
    {
        if (wanderPoints == null || wanderPoints.Length == 0)
        {
            return null;
        }
        return wanderPoints;
    }

    public Vector2Int RespawnTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        return context.respawnCell;
    }


}
