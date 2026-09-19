using System.Numerics;
using Raylib_cs;

namespace CoreShift.Game;

/// <summary>
/// Loads and draws top-down character sprites (Kenney "Topdown Shooter", CC0). Falls back
/// gracefully: if no textures are found, <see cref="Enabled"/> is false and callers use the
/// procedural figures instead.
/// </summary>
public sealed class SpriteRenderer : IDisposable
{
    private static readonly string[] CharacterFiles =
    {
        "survivor1_gun",
        "zoimbie1_hold",
        "robot1_stand",
        "robot1_gun",
    };

    private readonly Dictionary<string, Texture2D> _textures = new();

    public SpriteRenderer()
    {
        string? directory = FindCharacterDirectory();
        if (directory is null) return;

        foreach (var name in CharacterFiles)
        {
            string path = Path.Combine(directory, name + ".png");
            if (!File.Exists(path)) continue;
            _textures[name] = Raylib.LoadTexture(path);
        }
    }

    public bool Enabled => _textures.Count > 0;

    public bool Has(string name) => _textures.ContainsKey(name);

    /// <summary>Draws a character centered at <paramref name="screenPosition"/>, facing the angle.</summary>
    public void Draw(string name, Vector2 screenPosition, float angleDegrees, float targetHeight,
        Color tint, bool moving, float phase)
    {
        if (!_textures.TryGetValue(name, out var texture)) return;

        float scale = targetHeight / texture.Height;
        float width = texture.Width * scale;
        float height = texture.Height * scale;

        // Kenney top-down sprites face "east" (gun points right), matching the screen angle.
        float rotation = angleDegrees;
        if (moving) rotation += MathF.Sin(phase * 2f) * 3f;

        var position = screenPosition;
        if (moving) position.Y += MathF.Sin(phase * 2f) * targetHeight * 0.04f;

        var source = new Rectangle(0f, 0f, texture.Width, texture.Height);
        var destination = new Rectangle(position.X, position.Y, width, height);
        var origin = new Vector2(width * 0.5f, height * 0.5f);

        Raylib.DrawTexturePro(texture, source, destination, origin, rotation, tint);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values) Raylib.UnloadTexture(texture);
        _textures.Clear();
    }

    private static string? FindCharacterDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 8 && directory is not null; i++)
        {
            string candidate = Path.Combine(directory.FullName, "assets", "characters");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        return null;
    }
}
