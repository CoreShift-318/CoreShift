using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Spatial;

public sealed class SpatialHashGrid
{
    private readonly Dictionary<long, List<Entity>> _cells = new();
    private readonly List<long> _usedKeys = new();

    public SpatialHashGrid(float cellSize)
    {
        if (cellSize <= 0f) cellSize = 1f;
        CellSize = cellSize;
    }

    public float CellSize { get; }

    public void Clear()
    {
        for (int i = 0; i < _usedKeys.Count; i++)
        {
            _cells[_usedKeys[i]].Clear();
        }
        _usedKeys.Clear();
    }

    public void Insert(Entity entity, Vec2 position)
    {
        long key = Key(position);
        if (!_cells.TryGetValue(key, out var bucket))
        {
            bucket = new List<Entity>(8);
            _cells[key] = bucket;
        }
        if (bucket.Count == 0) _usedKeys.Add(key);
        bucket.Add(entity);
    }

    public int Query(Vec2 center, float radius, List<Entity> results)
    {
        results.Clear();
        int minX = CellCoord(center.X - radius);
        int maxX = CellCoord(center.X + radius);
        int minY = CellCoord(center.Y - radius);
        int maxY = CellCoord(center.Y + radius);

        for (int cy = minY; cy <= maxY; cy++)
        {
            for (int cx = minX; cx <= maxX; cx++)
            {
                if (!_cells.TryGetValue(Pack(cx, cy), out var bucket)) continue;
                for (int i = 0; i < bucket.Count; i++)
                {
                    results.Add(bucket[i]);
                }
            }
        }
        return results.Count;
    }

    private long Key(Vec2 position) => Pack(CellCoord(position.X), CellCoord(position.Y));

    private int CellCoord(float value) => (int)MathF.Floor(value / CellSize);

    private static long Pack(int cellX, int cellY) => ((long)cellX << 32) ^ (uint)cellY;
}
