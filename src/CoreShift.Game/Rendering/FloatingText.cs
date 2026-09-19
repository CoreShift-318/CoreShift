using CoreShift.Core.Math;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class FloatingTextSystem
{
    private struct Entry
    {
        public Vec2 Position;
        public string Text;
        public Color Color;
        public float Life;
        public float MaxLife;
        public int Size;
    }

    private readonly List<Entry> _entries = new();

    public int Count => _entries.Count;

    public void Add(Vec2 position, string text, Color color, int size, float life = 0.7f)
    {
        _entries.Add(new Entry
        {
            Position = position,
            Text = text,
            Color = color,
            Life = life,
            MaxLife = life,
            Size = size,
        });
    }

    public void Clear() => _entries.Clear();

    public void Update(float dt)
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var entry = _entries[i];
            entry.Life -= dt;
            entry.Position = new Vec2(entry.Position.X, entry.Position.Y + dt * 1.6f);

            if (entry.Life <= 0f)
            {
                int last = _entries.Count - 1;
                _entries[i] = _entries[last];
                _entries.RemoveAt(last);
                continue;
            }
            _entries[i] = entry;
        }
    }

    public void Draw(GameCamera camera)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            float t = entry.MaxLife <= 0f ? 0f : entry.Life / entry.MaxLife;
            byte alpha = (byte)(255 * t);
            var screen = camera.ToScreen(entry.Position);
            int width = Raylib.MeasureText(entry.Text, entry.Size);
            Raylib.DrawText(entry.Text, (int)(screen.X - width * 0.5f), (int)screen.Y, entry.Size, Palette.Alpha(entry.Color, alpha));
        }
    }
}
