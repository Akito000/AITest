using System.Collections.Generic;
using UnityEngine;

public static class GridPathfinder
{
    private static readonly MoveDirection[] AllDirections =
    {
        MoveDirection.UP, MoveDirection.DOWN, MoveDirection.LEFT, MoveDirection.RIGHT,
    };

    public static MoveDirection FindNextDirection(Vector2Int from, Vector2Int to, Stage stage, MoveDirection forbidden = MoveDirection.NONE)
    {
        if (from == to) return MoveDirection.NONE;

        var queue = new Queue<(Vector2Int cell, MoveDirection firstDir)>();
        var visited = new HashSet<Vector2Int> { from };

        foreach (var dir in AllDirections)
        {
            if (dir == forbidden) continue;
            if (!stage.CanMoveTo(from, dir)) continue;
            var next = stage.GetAdjacentCell(from, dir);

            //public Vector2Int GetAdjacentCell(Vector2Int currentCell, MoveDirection direction)
            //{
            //    return currentCell + MoveDirectionUtility.ToVector2Int(direction);
            //}


            if (visited.Add(next))
            {
                if (next == to) return dir;
                queue.Enqueue((next, dir));
            }
        }

        while (queue.Count > 0)
        {
            var (cell, firstDir) = queue.Dequeue();
            foreach (var dir in AllDirections)
            {
                if (!stage.CanMoveTo(cell, dir)) continue;
                var next = stage.GetAdjacentCell(cell, dir);
                if (visited.Add(next))
                {
                    if (next == to) return firstDir;
                    queue.Enqueue((next, firstDir));
                }
            }
        }

        // パスが見つからないときは進める任意の方向
        return FindAnyValidDirection(from, stage, forbidden);
    }

    /// <summary>threat から最も遠ざかる方向を返す（いじけ逃げ用）</summary>
    public static MoveDirection FindFleeDirection(Vector2Int from, Vector2Int threat, Stage stage, MoveDirection forbidden = MoveDirection.NONE)
    {
        MoveDirection best = MoveDirection.NONE;
        float maxDist = -1f;

        foreach (var dir in AllDirections)
        {
            if (dir == forbidden) continue;
            if (!stage.CanMoveTo(from, dir)) continue;
            float dist = (stage.GetAdjacentCell(from, dir) - threat).sqrMagnitude;
            if (dist > maxDist) { maxDist = dist; best = dir; }
        }

        return best != MoveDirection.NONE ? best : FindAnyValidDirection(from, stage, forbidden);
    }

    /// <summary>進める任意の方向（フォールバック）</summary>
    public static MoveDirection FindAnyValidDirection(Vector2Int from, Stage stage, MoveDirection forbidden = MoveDirection.NONE)
    {
        foreach (var dir in AllDirections)
            if (dir != forbidden && stage.CanMoveTo(from, dir)) return dir;
        // どうしても無いときだけ逆走を許可
        foreach (var dir in AllDirections)
            if (stage.CanMoveTo(from, dir)) return dir;
        return MoveDirection.NONE;
    }

    /// <summary>逆方向を返す（逆走禁止ルール用）</summary>
    public static MoveDirection Opposite(MoveDirection dir) => dir switch
    {
        MoveDirection.UP => MoveDirection.DOWN,
        MoveDirection.DOWN => MoveDirection.UP,
        MoveDirection.LEFT => MoveDirection.RIGHT,
        MoveDirection.RIGHT => MoveDirection.LEFT,
        _ => MoveDirection.NONE,
    };
}