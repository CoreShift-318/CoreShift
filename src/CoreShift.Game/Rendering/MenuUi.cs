using CoreShift.Core.Upgrades;
using Raylib_cs;

namespace CoreShift.Game;

public static class MenuUi
{
    public static void DrawTitle(string title, int y)
    {
        const int size = 66;
        int width = Raylib.GetScreenWidth();
        int textWidth = Raylib.MeasureText(title, size);
        int x = (width - textWidth) / 2;

        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawText(title, x - 3, y, size, Palette.Alpha(Palette.Accent, 70));
        Raylib.DrawText(title, x + 3, y, size, Palette.Alpha(Palette.Player, 70));
        Raylib.EndBlendMode();
        Raylib.DrawText(title, x, y, size, Palette.Text);
    }

    public static void DrawSubtitle(string text, int y)
    {
        const int size = 20;
        int width = Raylib.GetScreenWidth();
        int textWidth = Raylib.MeasureText(text, size);
        Raylib.DrawText(text, (width - textWidth) / 2, y, size, Palette.TextDim);
    }

    public static void DrawList(IReadOnlyList<string> items, int selected, int startY)
    {
        const int size = 30;
        int width = Raylib.GetScreenWidth();

        for (int i = 0; i < items.Count; i++)
        {
            bool isSelected = i == selected;
            string text = (isSelected ? "> " : "  ") + items[i];
            int textWidth = Raylib.MeasureText(text, size);
            int x = (width - textWidth) / 2;
            int y = startY + i * 48;
            Color color = isSelected ? Palette.Player : Palette.TextDim;

            if (isSelected)
            {
                Raylib.BeginBlendMode(BlendMode.Additive);
                Raylib.DrawText(text, x, y, size, Palette.Alpha(Palette.Player, 90));
                Raylib.EndBlendMode();
            }
            Raylib.DrawText(text, x, y, size, color);
        }
    }

    public static void DrawPanel(Rectangle rectangle, Color border)
    {
        Raylib.DrawRectangleRec(rectangle, Palette.Alpha(Palette.Background, 235));
        Raylib.DrawRectangleLinesEx(rectangle, 2f, border);
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawRectangleLinesEx(new Rectangle(rectangle.X - 3, rectangle.Y - 3, rectangle.Width + 6, rectangle.Height + 6), 1f, Palette.Alpha(border, 70));
        Raylib.EndBlendMode();
    }

    public static void DrawUpgradeCards(IReadOnlyList<UpgradeDef> offers, int selected)
    {
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        const float cardWidth = 300f;
        const float cardHeight = 360f;
        const float gap = 32f;
        float totalWidth = offers.Count * cardWidth + (offers.Count - 1) * gap;
        float startX = (width - totalWidth) * 0.5f;
        float y = (height - cardHeight) * 0.5f + 20f;

        for (int i = 0; i < offers.Count; i++)
        {
            var offer = offers[i];
            bool isSelected = i == selected;
            float x = startX + i * (cardWidth + gap);
            var rectangle = new Rectangle(x, y, cardWidth, cardHeight);
            Color rarity = Palette.RarityColor(offer.Rarity);
            Color border = isSelected ? Palette.Text : Palette.Alpha(rarity, 160);

            DrawPanel(rectangle, border);

            if (isSelected)
            {
                Raylib.BeginBlendMode(BlendMode.Additive);
                Raylib.DrawRectangleRec(rectangle, Palette.Alpha(rarity, 26));
                Raylib.EndBlendMode();
            }

            Raylib.DrawText($"{i + 1}", (int)(x + 20), (int)(y + 16), 28, rarity);
            Raylib.DrawText(offer.Name, (int)(x + 20), (int)(y + 60), 30, Palette.Text);

            DrawWrapped(offer.Description, (int)(x + 20), (int)(y + 110), (int)(cardWidth - 40), 20, Palette.TextDim);

            string stacks = $"Rarity {offer.Rarity}   Max {offer.MaxStacks}";
            Raylib.DrawText(stacks, (int)(x + 20), (int)(y + cardHeight - 40), 16, Palette.TextDim);
        }
    }

    private static void DrawWrapped(string text, int x, int y, int maxWidth, int size, Color color)
    {
        string[] words = text.Split(' ');
        string line = string.Empty;
        int lineY = y;

        foreach (var word in words)
        {
            string candidate = line.Length == 0 ? word : line + " " + word;
            if (Raylib.MeasureText(candidate, size) > maxWidth && line.Length > 0)
            {
                Raylib.DrawText(line, x, lineY, size, color);
                lineY += size + 6;
                line = word;
            }
            else
            {
                line = candidate;
            }
        }

        if (line.Length > 0) Raylib.DrawText(line, x, lineY, size, color);
    }
}
