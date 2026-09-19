namespace CoreShift.Core.Rng;

public sealed class DeterministicRng
{
    private uint _state;

    public DeterministicRng(uint seed)
    {
        Reseed(seed);
    }

    public uint State => _state;

    public void Reseed(uint seed)
    {
        _state = seed == 0u ? 0x9E3779B9u : seed;
    }

    public uint NextUInt()
    {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        uint span = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt() % span);
    }

    public float NextFloat() => (NextUInt() >> 8) * (1.0f / 16777216.0f);

    public float Range(float min, float max) => min + (max - min) * NextFloat();

    public bool Chance(float probability) => NextFloat() < probability;
}
