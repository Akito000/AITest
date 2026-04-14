using UnityEngine;

public interface IGhostTargetResolver
{
    Vector2Int ChaseTargetCell(in EnemyContext context, EnemyMemory memory);
    Vector2Int WanderTargetCell(in EnemyContext context, EnemyMemory memory);
    Vector2Int EscapeTargetCell(in EnemyContext context, EnemyMemory memory);
    Vector2Int RespawnTargetCell(in EnemyContext context, EnemyMemory memory);
}
