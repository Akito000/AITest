using UnityEngine;

public class PinkEnemy : MonoBehaviour, IGhostTargetResolver
{
    [SerializeField] private Vector2Int wanderCell = new Vector2Int(0, 0);

    [Header("先読みするマス数")]
    [SerializeField] private int lookAheadCells = 4;

    public Vector2Int ChaseTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        // プレイヤーの向いている方向の4マス先を狙う
        return context.playerCell + context.playerForward * lookAheadCells;
    }

    public Vector2Int EscapeTargetCell(in EnemyContext context, EnemyMemory memory)
    {
        // 逃走時はプレイヤーから離れる方向へ
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
