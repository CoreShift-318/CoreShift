using CoreShift.Core.Math;
using CoreShift.Core.Rng;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class ParticleSystem
{
    private struct Particle
    {
        public Vec2 Position;
        public Vec2 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
        public Color Color;
    }

    private readonly List<Particle> _particles = new();
    private readonly DeterministicRng _rng = new(0xC0FFEEu);

    public int Count => _particles.Count;

    public void EmitBurst(Vec2 position, Color color, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = _rng.Range(0f, MathF.Tau);
            float magnitude = speed * _rng.Range(0.35f, 1f);
            _particles.Add(new Particle
            {
                Position = position,
                Velocity = new Vec2(MathF.Cos(angle) * magnitude, MathF.Sin(angle) * magnitude),
                MaxLife = _rng.Range(0.22f, 0.55f),
                Life = 0f,
                Size = _rng.Range(1.5f, 3.6f),
                Color = color,
            });
            var p = _particles[^1];
            p.Life = p.MaxLife;
            _particles[^1] = p;
        }
    }

    public void Update(float dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Life -= dt;
            if (p.Life <= 0f)
            {
                int last = _particles.Count - 1;
                _particles[i] = _particles[last];
                _particles.RemoveAt(last);
                continue;
            }

            p.Position += p.Velocity * dt;
            p.Velocity = p.Velocity * 0.94f;
            _particles[i] = p;
        }
    }

    public void Draw(GameCamera camera)
    {
        Raylib.BeginBlendMode(BlendMode.Additive);
        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            float t = p.MaxLife <= 0f ? 0f : p.Life / p.MaxLife;
            byte alpha = (byte)(200 * t);
            float radius = MathF.Max(0.5f, p.Size * t);
            Raylib.DrawCircleV(camera.ToScreen(p.Position), radius, Palette.Alpha(p.Color, alpha));
        }
        Raylib.EndBlendMode();
    }
}
