using UnityEngine;

public class PinkEnemy : MonoBehaviour, IGhostTargetResolver, IWanderProvider
{
    [Header("先読みするマス数")]
    [SerializeField] private int lookAheadCells = 4;

    [Header("徘徊ポイント")]
    [SerializeField]
    private Vector2Int[] wanderPoints =
    {
        new Vector2Int(1, 18),
        new Vector2Int(4, 18),
        new Vector2Int(4, 20),
        new Vector2Int(1, 20),
    };

    public Vector2Int ChaseTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        // プレイヤーの向いている方向の4マス先
        return context.playerCell + context.playerForward * lookAheadCells;
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
