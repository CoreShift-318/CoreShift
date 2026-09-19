namespace CoreShift.Core.Ecs;

public readonly struct Entity : IEquatable<Entity>
{
    public readonly int Id;
    public readonly uint Generation;

    public Entity(int id, uint generation)
    {
        Id = id;
        Generation = generation;
    }

    public static readonly Entity Null = new(-1, 0);

    public bool IsNull => Id < 0;

    public bool Equals(Entity other) => Id == other.Id && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is Entity other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Id, Generation);
    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    public override string ToString() => $"Entity({Id}:{Generation})";
}
