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
