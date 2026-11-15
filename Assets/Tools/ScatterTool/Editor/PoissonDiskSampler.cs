using System.Collections.Generic;
using UnityEngine;

namespace ScatterTool
{
    /// <summary>
    /// Bridsonのポアソンディスクサンプリング。
    /// どの2点も minDistance 以上離れ、新しい点は既存の点から minDistance〜maxDistance の距離に生まれる。
    /// 座標は (0,0)〜size の矩形内。
    /// </summary>
    public static class PoissonDiskSampler
    {
        // これを超えるグリッドはメモリを食いすぎるので作らない
        public const long MaxGridCells = 20_000_000;

        public static long EstimateGridCells(Vector2 size, float minDistance)
        {
            if (minDistance <= 0f) return long.MaxValue;
            float cellSize = minDistance / Mathf.Sqrt(2f);
            return (long)Mathf.CeilToInt(size.x / cellSize) * Mathf.CeilToInt(size.y / cellSize);
        }

        public static List<Vector2> Sample(Vector2 size, float minDistance, float maxDistance, int maxPoints, System.Random rng, int attemptsPerPoint = 30)
        {
            var points = new List<Vector2>();
            if (size.x <= 0f || size.y <= 0f || minDistance <= 0f || maxPoints <= 0) return points;
            if (EstimateGridCells(size, minDistance) > MaxGridCells) return points;

            maxDistance = Mathf.Max(minDistance, maxDistance);
            float cellSize = minDistance / Mathf.Sqrt(2f);
            int gridW = Mathf.CeilToInt(size.x / cellSize);
            int gridH = Mathf.CeilToInt(size.y / cellSize);
            var grid = new int[gridW * gridH];
            for (int i = 0; i < grid.Length; i++) grid[i] = -1;

            var active = new List<int>();
            float minSq = minDistance * minDistance;
            float maxSq = maxDistance * maxDistance;

            AddPoint(new Vector2((float)rng.NextDouble() * size.x, (float)rng.NextDouble() * size.y));

            while (active.Count > 0 && points.Count < maxPoints)
            {
                int activeIndex = rng.Next(active.Count);
                Vector2 origin = points[active[activeIndex]];
                bool found = false;

                for (int k = 0; k < attemptsPerPoint; k++)
                {
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    // 円環の面積に対して一様になるように半径を決める
                    float radius = Mathf.Sqrt(Mathf.Lerp(minSq, maxSq, (float)rng.NextDouble()));
                    Vector2 candidate = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                    if (IsValid(candidate))
                    {
                        AddPoint(candidate);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // 入れ替えて末尾を消す(順番は関係ないのでO(1)で消す)
                    active[activeIndex] = active[active.Count - 1];
                    active.RemoveAt(active.Count - 1);
                }
            }

            return points;

            void AddPoint(Vector2 p)
            {
                points.Add(p);
                active.Add(points.Count - 1);
                grid[CellX(p) + CellY(p) * gridW] = points.Count - 1;
            }

            bool IsValid(Vector2 p)
            {
                if (p.x < 0f || p.y < 0f || p.x >= size.x || p.y >= size.y) return false;

                int cx = CellX(p);
                int cy = CellY(p);
                int x0 = Mathf.Max(0, cx - 2), x1 = Mathf.Min(gridW - 1, cx + 2);
                int y0 = Mathf.Max(0, cy - 2), y1 = Mathf.Min(gridH - 1, cy + 2);

                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int index = grid[x + y * gridW];
                        if (index >= 0 && (points[index] - p).sqrMagnitude < minSq) return false;
                    }
                }
                return true;
            }

            int CellX(Vector2 p) => Mathf.Min(gridW - 1, (int)(p.x / cellSize));
            int CellY(Vector2 p) => Mathf.Min(gridH - 1, (int)(p.y / cellSize));
        }
    }
}
