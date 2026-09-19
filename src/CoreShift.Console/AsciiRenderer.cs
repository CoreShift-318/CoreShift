using System.Text;
using CoreShift.Core;
using CoreShift.Core.Ecs;

namespace CoreShift.App;

public static class AsciiRenderer
{
    public static string Render(World world, int width = 60, int height = 30)
    {
        if (width < 2) width = 2;
        if (height < 2) height = 2;

        var grid = new char[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) grid[y, x] = '.';
        }

        foreach (var entity in world.Entities.With<EnemyTag>())
        {
            Plot(world, grid, width, height, entity, 'e');
        }

        foreach (var entity in world.Entities.With<ProjectileTag>())
        {
            Plot(world, grid, width, height, entity, '*');
        }

        if (world.HasPlayer) Plot(world, grid, width, height, world.Player, '@');

        var builder = new StringBuilder((width + 1) * height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) builder.Append(grid[y, x]);
            builder.Append('\n');
        }
        return builder.ToString();
    }

    private static void Plot(World world, char[,] grid, int width, int height, Entity entity, char glyph)
    {
        if (!world.Entities.Has<Transform2>(entity)) return;

        var transform = world.Entities.Get<Transform2>(entity);
        float halfWidth = world.Arena.Width * 0.5f;
        float halfHeight = world.Arena.Height * 0.5f;

        int column = (int)MathF.Round((transform.X + halfWidth) / world.Arena.Width * (width - 1));
        int row = (int)MathF.Round((halfHeight - transform.Y) / world.Arena.Height * (height - 1));

        if (column < 0 || column >= width || row < 0 || row >= height) return;
        grid[row, column] = glyph;
    }
}
