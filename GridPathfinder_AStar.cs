
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// グリッド上で A* による経路探索を行う静的クラス
/// 
/// このクラスの役割:
/// - start から goal までの経路を求める
/// - または「次の1手」だけ返す
/// - 敵ごとの移動コスト戦略(ICellCostStrategy)を差し替えられる
/// </summary>
public static class GridPathfinder
{
    /// <summary>
    /// 探索で使う4方向
    /// パックマン系なので斜め移動はなし
    /// </summary>
    private static readonly MoveDirection[] AllDirections =
    {
        MoveDirection.UP,
        MoveDirection.DOWN,
        MoveDirection.LEFT,
        MoveDirection.RIGHT,
    };

    /// <summary>
    /// openList に入れる探索ノード
    /// 
    /// Cell  : このノードが表すセル座標
    /// GCost : start からここまで実際にかかったコスト
    /// FCost : GCost + Heuristic(ここからgoalまでの予想コスト)
    /// </summary>
    private class NodeRecord
    {
        public Vector2Int Cell;
        public float GCost;
        public float FCost;
    }

    /// <summary>
    /// A* で start から goal までの経路を求める
    /// 
    /// outPath には
    /// start -> ... -> goal
    /// の順でセル列を入れる
    /// </summary>
    public static bool FindPathAStar(
        Vector2Int start,
        Vector2Int goal,
        Stage stage,
        EnemyContext context,
        ICellCostStrategy costStrategy,
        MoveDirection forbidden,
        List<Vector2Int> outPath)
    {
        // 出力先がないと結果を返せない
        if (outPath == null)
        {
            return false;
        }

        outPath.Clear();

        // 開始地点とゴールが同じなら、経路はその1点だけでよい
        if (start == goal)
        {
            outPath.Add(start);
            return true;
        }

        // コスト戦略が未指定なら、全セルコスト1のデフォルトを使う
        if (costStrategy == null)
        {
            costStrategy = new DefaultCellCostStrategy();
        }

        // openList:
        //   まだ調べ終わっていない候補ノード
        // closedSet:
        //   調べ終わったセル
        // bestCostMap:
        //   各セルに到達した時の最良Gコスト
        // parentMap:
        //   経路復元用。「このセルへ来る直前のセル」
        var openList = new List<NodeRecord>();
        var closedSet = new HashSet<Vector2Int>();
        var bestCostMap = new Dictionary<Vector2Int, float>();
        var parentMap = new Dictionary<Vector2Int, Vector2Int>();

        // 開始地点を探索候補に入れる
        openList.Add(new NodeRecord
        {
            Cell = start,
            GCost = 0f,
            FCost = Heuristic(start, goal)
        });

        bestCostMap[start] = 0f;

        // openList が空になるまで探索を続ける
        while (openList.Count > 0)
        {
            // openList の中から FCost が最小のノードを選ぶ
            // 同点なら GCost が小さい方を優先
            int bestIndex = 0;
            float bestFCost = openList[0].FCost;
            float bestGCost = openList[0].GCost;

            for (int i = 1; i < openList.Count; i++)
            {
                NodeRecord candidate = openList[i];

                if (candidate.FCost < bestFCost ||
                    (Mathf.Approximately(candidate.FCost, bestFCost) && candidate.GCost < bestGCost))
                {
                    bestIndex = i;
                    bestFCost = candidate.FCost;
                    bestGCost = candidate.GCost;
                }
            }

            // 最良候補を取り出す
            NodeRecord current = openList[bestIndex];
            openList.RemoveAt(bestIndex);

            // 既に closed 済みならスキップ
            if (!closedSet.Add(current.Cell))
            {
                continue;
            }

            // ゴール到達なら parentMap をたどって経路復元
            if (current.Cell == goal)
            {
                ReconstructPath(start, goal, parentMap, outPath);
                return outPath.Count > 0;
            }

            // 現在セルから4方向へ展開
            foreach (MoveDirection dir in AllDirections)
            {
                // 開始地点の「最初の1手」だけ逆走を禁止
                // これにより、今向いている方向の真逆へ即Uターンしにくくしている
                if (current.Cell == start && dir == forbidden)
                {
                    continue;
                }

                // その方向へ進めないなら無視
                if (!stage.CanMoveTo(current.Cell, dir))
                {
                    continue;
                }

                Vector2Int nextCell = stage.GetAdjacentCell(current.Cell, dir);

                // 既に調査済みなら無視
                if (closedSet.Contains(nextCell))
                {
                    continue;
                }

                // 「nextCell に入るコスト」を戦略オブジェクトから取得
                // ここを差し替えると
                // - ドットを好む
                // - 危険地帯を避ける
                // - 敵ごとに性格を変える
                // といったことができる
                float enterCost = costStrategy.GetEnterCost(current.Cell, nextCell, stage, context);

                // A* は負コストに弱いので、最低でもごく小さい正の値にしておく
                enterCost = Mathf.Max(0.001f, enterCost);

                // start から nextCell までの新しい実コスト
                float newGCost = current.GCost + enterCost;

                // 既にもっと安いコストで到達済みなら更新しない
                if (bestCostMap.TryGetValue(nextCell, out float oldGCost) &&
                    oldGCost <= newGCost)
                {
                    continue;
                }

                // より良い経路として記録し直す
                bestCostMap[nextCell] = newGCost;
                parentMap[nextCell] = current.Cell;

                // F = G + H
                openList.Add(new NodeRecord
                {
                    Cell = nextCell,
                    GCost = newGCost,
                    FCost = newGCost + Heuristic(nextCell, goal)
                });
            }
        }

        // openList が尽きた = ゴールへ到達できなかった
        return false;
    }

    /// <summary>
    /// A* で経路を求め、その結果から「次の1手」だけ返す
    /// 
    /// 使いどころ:
    /// - 敵AIが毎回1マスぶんの進行方向だけ欲しいとき
    /// </summary>
    public static MoveDirection FindNextDirectionAStar(
        Vector2Int start,
        Vector2Int goal,
        Stage stage,
        EnemyContext context,
        ICellCostStrategy costStrategy,
        MoveDirection forbidden,
        List<Vector2Int> debugPath)
    {
        if (FindPathAStar(start, goal, stage, context, costStrategy, forbidden, debugPath))
        {
            // 経路が start, next, ... と2点以上あるなら
            // 2番目のセルが「次に進むべきセル」
            if (debugPath.Count >= 2)
            {
                return ToDirection(debugPath[1] - start);
            }

            // start == goal なら移動不要
            return MoveDirection.NONE;
        }

        // 探索失敗時はデバッグ経路を start のみにしておく
        debugPath?.Clear();
        debugPath?.Add(start);

        // 完全停止を避けるため、行ける方向へフォールバック
        return FindAnyValidDirection(start, stage, forbidden);
    }

    /// <summary>
    /// parentMap をたどって経路を復元する
    /// 
    /// parentMap は「子 -> 親」の辞書なので、
    /// goal から start へ逆順にたどり、最後に Reverse して正順へ直す
    /// </summary>
    private static void ReconstructPath(
        Vector2Int start,
        Vector2Int goal,
        Dictionary<Vector2Int, Vector2Int> parentMap,
        List<Vector2Int> outPath)
    {
        outPath.Clear();

        Vector2Int current = goal;
        outPath.Add(current);

        while (current != start)
        {
            // 途中で親が見つからなければ復元失敗
            if (!parentMap.TryGetValue(current, out Vector2Int parent))
            {
                outPath.Clear();
                return;
            }

            current = parent;
            outPath.Add(current);
        }

        // 今は goal -> ... -> start の順なので反転
        outPath.Reverse();
    }

    /// <summary>
    /// A* が失敗したときのフォールバック用
    /// 
    /// まず forbidden 以外を探し、
    /// なければ forbidden も含めて、とにかく進める方向を返す
    /// </summary>
    public static MoveDirection FindAnyValidDirection(
        Vector2Int from,
        Stage stage,
        MoveDirection forbidden = MoveDirection.NONE)
    {
        // できれば逆走以外を選ぶ
        foreach (MoveDirection dir in AllDirections)
        {
            if (dir != forbidden && stage.CanMoveTo(from, dir))
            {
                return dir;
            }
        }

        // どうしても無ければ逆方向も許可
        foreach (MoveDirection dir in AllDirections)
        {
            if (stage.CanMoveTo(from, dir))
            {
                return dir;
            }
        }

        return MoveDirection.NONE;
    }

    /// <summary>
    /// 逆方向を返す
    /// 例: UP -> DOWN
    /// </summary>
    public static MoveDirection Opposite(MoveDirection dir) => dir switch
    {
        MoveDirection.UP => MoveDirection.DOWN,
        MoveDirection.DOWN => MoveDirection.UP,
        MoveDirection.LEFT => MoveDirection.RIGHT,
        MoveDirection.RIGHT => MoveDirection.LEFT,
        _ => MoveDirection.NONE,
    };

    /// <summary>
    /// セル差分を MoveDirection に変換する
    /// </summary>
    private static MoveDirection ToDirection(Vector2Int delta)
    {
        if (delta == Vector2Int.up) return MoveDirection.UP;
        if (delta == Vector2Int.down) return MoveDirection.DOWN;
        if (delta == Vector2Int.left) return MoveDirection.LEFT;
        if (delta == Vector2Int.right) return MoveDirection.RIGHT;
        return MoveDirection.NONE;
    }

    /// <summary>
    /// ヒューリスティック関数
    /// 
    /// 今回は4方向移動だけなのでマンハッタン距離を使う
    /// A* の「ゴールまであとどれくらいか」の目安
    /// </summary>
    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// コスト戦略が未指定の時に使うデフォルト
    /// 全セルの進入コストを 1 とみなす
    /// 
    /// つまりこの場合は
    /// 「重み付きA*」というより、ほぼ通常の最短経路探索になる
    /// </summary>
    private class DefaultCellCostStrategy : ICellCostStrategy
    {
        public float GetEnterCost(Vector2Int from, Vector2Int to, Stage stage, EnemyContext context)
        {
            return 1.0f;
        }
    }
}




using System.Collections.Generic;
using UnityEngine;

public static class GridPathfinder
{
    private static readonly MoveDirection[] AllDirections =
    {
        MoveDirection.UP,
        MoveDirection.DOWN,
        MoveDirection.LEFT,
        MoveDirection.RIGHT,
    };

    private class NodeRecord
    {
        public Vector2Int Cell;
        public float GCost;
        public float FCost;
    }

    /// <summary>
    /// A* で start -> goal の経路を求める
    /// 経路が見つかったら outPath に start, ..., goal の順で格納する
    /// </summary>
    public static bool FindPathAStar(
        Vector2Int start,
        Vector2Int goal,
        Stage stage,
        EnemyContext context,
        ICellCostStrategy costStrategy,
        MoveDirection forbidden,
        List<Vector2Int> outPath)
    {
        if (outPath == null)
        {
            return false;
        }

        outPath.Clear();

        if (start == goal)
        {
            outPath.Add(start);
            return true;
        }

        if (costStrategy == null)
        {
            costStrategy = new DefaultCellCostStrategy();
        }

        var openList = new List<NodeRecord>();
        var closedSet = new HashSet<Vector2Int>();
        var bestCostMap = new Dictionary<Vector2Int, float>();
        var parentMap = new Dictionary<Vector2Int, Vector2Int>();

        openList.Add(new NodeRecord
        {
            Cell = start,
            GCost = 0f,
            FCost = Heuristic(start, goal)
        });

        bestCostMap[start] = 0f;

        while (openList.Count > 0)
        {
            int bestIndex = 0;
            float bestFCost = openList[0].FCost;
            float bestGCost = openList[0].GCost;

            for (int i = 1; i < openList.Count; i++)
            {
                NodeRecord candidate = openList[i];

                if (candidate.FCost < bestFCost ||
                    (Mathf.Approximately(candidate.FCost, bestFCost) && candidate.GCost < bestGCost))
                {
                    bestIndex = i;
                    bestFCost = candidate.FCost;
                    bestGCost = candidate.GCost;
                }
            }

            NodeRecord current = openList[bestIndex];
            openList.RemoveAt(bestIndex);

            if (!closedSet.Add(current.Cell))
            {
                continue;
            }

            if (current.Cell == goal)
            {
                ReconstructPath(start, goal, parentMap, outPath);
                return outPath.Count > 0;
            }

            foreach (MoveDirection dir in AllDirections)
            {
                // 開始地点の 1 手目だけ逆走禁止
                if (current.Cell == start && dir == forbidden)
                {
                    continue;
                }

                if (!stage.CanMoveTo(current.Cell, dir))
                {
                    continue;
                }

                Vector2Int nextCell = stage.GetAdjacentCell(current.Cell, dir);

                if (closedSet.Contains(nextCell))
                {
                    continue;
                }

                float enterCost = costStrategy.GetEnterCost(current.Cell, nextCell, stage, context);

                // A* は負のコストに弱いので、最低でもごく小さい正の値に丸める
                enterCost = Mathf.Max(0.001f, enterCost);

                float newGCost = current.GCost + enterCost;

                if (bestCostMap.TryGetValue(nextCell, out float oldGCost) && oldGCost <= newGCost)
                {
                    continue;
                }

                bestCostMap[nextCell] = newGCost;
                parentMap[nextCell] = current.Cell;

                openList.Add(new NodeRecord
                {
                    Cell = nextCell,
                    GCost = newGCost,
                    FCost = newGCost + Heuristic(nextCell, goal)
                });
            }
        }

        return false;
    }

    /// <summary>
    /// A* で次の 1 手だけ欲しいとき用
    /// </summary>
    public static MoveDirection FindNextDirectionAStar(
        Vector2Int start,
        Vector2Int goal,
        Stage stage,
        EnemyContext context,
        ICellCostStrategy costStrategy,
        MoveDirection forbidden,
        List<Vector2Int> debugPath)
    {
        if (FindPathAStar(start, goal, stage, context, costStrategy, forbidden, debugPath))
        {
            if (debugPath.Count >= 2)
            {
                return ToDirection(debugPath[1] - start);
            }

            return MoveDirection.NONE;
        }

        debugPath?.Clear();
        debugPath?.Add(start);
        return FindAnyValidDirection(start, stage, forbidden);
    }

    private static void ReconstructPath(
        Vector2Int start,
        Vector2Int goal,
        Dictionary<Vector2Int, Vector2Int> parentMap,
        List<Vector2Int> outPath)
    {
        outPath.Clear();

        Vector2Int current = goal;
        outPath.Add(current);

        while (current != start)
        {
            if (!parentMap.TryGetValue(current, out Vector2Int parent))
            {
                outPath.Clear();
                return;
            }

            current = parent;
            outPath.Add(current);
        }

        outPath.Reverse();
    }

    public static MoveDirection FindAnyValidDirection(
        Vector2Int from,
        Stage stage,
        MoveDirection forbidden = MoveDirection.NONE)
    {
        foreach (MoveDirection dir in AllDirections)
        {
            if (dir != forbidden && stage.CanMoveTo(from, dir))
            {
                return dir;
            }
        }

        foreach (MoveDirection dir in AllDirections)
        {
            if (stage.CanMoveTo(from, dir))
            {
                return dir;
            }
        }

        return MoveDirection.NONE;
    }

    public static MoveDirection Opposite(MoveDirection dir) => dir switch
    {
        MoveDirection.UP => MoveDirection.DOWN,
        MoveDirection.DOWN => MoveDirection.UP,
        MoveDirection.LEFT => MoveDirection.RIGHT,
        MoveDirection.RIGHT => MoveDirection.LEFT,
        _ => MoveDirection.NONE,
    };

    private static MoveDirection ToDirection(Vector2Int delta)
    {
        if (delta == Vector2Int.up) return MoveDirection.UP;
        if (delta == Vector2Int.down) return MoveDirection.DOWN;
        if (delta == Vector2Int.left) return MoveDirection.LEFT;
        if (delta == Vector2Int.right) return MoveDirection.RIGHT;
        return MoveDirection.NONE;
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        // 4 方向移動なのでマンハッタン距離
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private class DefaultCellCostStrategy : ICellCostStrategy
    {
        public float GetEnterCost(Vector2Int from, Vector2Int to, Stage stage, EnemyContext context)
        {
            return 1.0f;
        }
    }
}
