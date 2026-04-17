using UnityEngine;

public class DotEater : MonoBehaviour, IGhostTargetResolver, IWanderProvider, ICellCostStrategy
{



    [SerializeField] private float pelletCellCost = 0.2f;

    [SerializeField] private float emptyCellCost = 1.5f;

    [SerializeField] private float minCost = 0.05f;

    public float GetEnterCost(Vector2Int from, Vector2Int to, Stage stage, EnemyContext context)

    {

        float cost = stage.HasPellet(to) ? pelletCellCost : emptyCellCost;

        // 念のため正の値を保証

        return Mathf.Max(minCost, cost);

    }

}













    [Header("徘徊ポイント")]
    [SerializeField]
    private Vector2Int[] wanderPoints =
    {
        new Vector2Int(16, 18),
        new Vector2Int(19, 18),
        new Vector2Int(19, 20),
        new Vector2Int(16, 20),
    };

    [Header("基本移動コスト")]
    [SerializeField] private float normalCost = 1.0f;

    [Header("進入先にドットがある時のコスト")]
    [SerializeField] private float pelletCost = 0.6f;

    [Header("周囲ドット1個あたりの割引量")]
    [SerializeField] private float nearbyPelletDiscount = 0.08f;

    [Header("周囲を見る半径(マンハッタン距離)")]
    [SerializeField] private int nearbyRadius = 2;

    [Header("最小コスト")]
    [SerializeField] private float minCost = 0.1f;






    /// <summary>
    /// A* が「このマスに入るコスト」を問い合わせる
    /// 
    /// - ドットがあるマスは安くする
    /// - 周囲にもドットが多いなら少し安くする
    /// </summary>
    public float GetEnterCost(Vector2Int from, Vector2Int to, Stage stage, EnemyContext context)
    {
        float cost = normalCost;

        // 進入先のマス自体にドットがあれば安くする
        if (stage.HasPellet(to))
        {
            cost = pelletCost;
        }

        // 周囲にドットが多い通路も少し優先する
        int nearbyPelletCount = CountNearbyPellets(stage, to, nearbyRadius);
        cost -= nearbyPelletCount * nearbyPelletDiscount;

        // A* 用に必ず正の値へ丸める
        return Mathf.Max(minCost, cost);
    }

    /// <summary>
    /// 追跡時の目標セル
    /// 
    /// まずはプレイヤー自身を目標にする。
    /// 道の選び方は GetEnterCost 側で差別化する。
    /// </summary>
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

    public Vector2Int[] GetWanderPoints()
    {
        if (wanderPoints == null || wanderPoints.Length == 0)
        {
            return null;
        }

        return wanderPoints;
    }

    /// <summary>
    /// 指定セルの周囲にあるドット数を数える
    /// マンハッタン距離で見る
    /// </summary>
    private int CountNearbyPellets(Stage stage, Vector2Int center, int radius)
    {
        int count = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                // マンハッタン距離で範囲制限
                if (Mathf.Abs(x) + Mathf.Abs(y) > radius)
                {
                    continue;
                }

                Vector2Int cell = new Vector2Int(center.x + x, center.y + y);

                // ステージ外は無視
                if (!stage.IsInside(cell))
                {
                    continue;
                }

                if (stage.HasPellet(cell))
                {
                    count++;
                }
            }
        }

        return count;
    }
}
