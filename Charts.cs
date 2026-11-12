using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class LineChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public string[] Labels { get; set; } = Array.Empty<string>();
    public Color AccentColor { get; set; } = Colors.Purple;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length < 2) return;
        float pl = 25, pb = 20;
        float w = dirtyRect.Width - pl, h = dirtyRect.Height - pb;
        float max = Values.Max(); if (max == 0) max = 1;
        max = (float)(Math.Ceiling(max / 10.0) * 10); if (max == 0) max = 10;

        canvas.FontSize = 9; canvas.FontColor = Colors.Gray;
        canvas.StrokeSize = 0.5f; canvas.StrokeColor = Colors.Gray.WithAlpha(0.2f);
        for (int i = 0; i <= 3; i++) {
            float y = h - (i * (h / 3)); float val = (i * (max / 3));
            canvas.DrawLine(pl, y, dirtyRect.Width, y);
            canvas.DrawString(val.ToString("F0"), 0, y - 5, pl - 5, 10, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        float stepX = w / (Values.Length - 1);
        var path = new PathF(); path.MoveTo(pl, h - (Values[0] / max * h));
        for (int i = 1; i < Values.Length; i++) path.LineTo(pl + i * stepX, h - (Values[i] / max * h));
        var fill = new PathF(path); fill.LineTo(dirtyRect.Width, h); fill.LineTo(pl, h); fill.Close();
        
        canvas.FillColor = AccentColor.WithAlpha(0.2f); canvas.FillPath(fill);
        canvas.StrokeSize = 3; canvas.StrokeColor = AccentColor; canvas.StrokeLineJoin = LineJoin.Round; canvas.DrawPath(path);

        canvas.FillColor = Colors.White;
        for (int i = 0; i < Values.Length; i++) {
            float x = pl + i * stepX, y = h - (Values[i] / max * h);
            canvas.FillCircle(x, y, 4);
            if (i < Labels.Length) { canvas.FontColor = Colors.Gray; canvas.DrawString(Labels[i], x - 15, h + 5, 30, 15, HorizontalAlignment.Center, VerticalAlignment.Top); }
        }
    }
}

public class DonutChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public Color[] Colors { get; set; } = Array.Empty<Color>();
    public Color BackgroundColor { get; set; } = Color.FromArgb("#201D28");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length == 0) return;
        float total = Values.Sum(); if (total == 0) return;
        float cx = dirtyRect.Center.X, cy = dirtyRect.Center.Y;
        float r = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2.2f;
        float ir = r * 0.65f;
        float start = -90;

        for (int i = 0; i < Values.Length; i++) {
            float sweep = (Values[i] / total) * 360;
            canvas.FillColor = Colors[i % Colors.Length];
            var p = new PathF(); p.MoveTo(cx, cy);
            p.AddArc(cx - r, cy - r, r * 2, r * 2, start, start + sweep, true); p.Close();
            canvas.FillPath(p); start += sweep;
        }
        canvas.FillColor = BackgroundColor; canvas.FillCircle(cx, cy, ir);
    }
}

public class CostChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public string[] Labels { get; set; } = Array.Empty<string>();
    public Color AccentColor { get; set; } = Colors.Purple;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length < 2) return;
        float pl = 25, pb = 20;
        float w = dirtyRect.Width - pl, h = dirtyRect.Height - pb;
        float max = Values.Max(); if (max == 0) max = 1;

        canvas.FontSize = 9; canvas.FontColor = Colors.Gray;
        canvas.StrokeSize = 0.5f; canvas.StrokeColor = Colors.Gray.WithAlpha(0.2f);
        
        // Оси
        canvas.DrawLine(pl, 0, pl, h); // Y
        canvas.DrawLine(pl, h, dirtyRect.Width, h); // X

        for (int i = 0; i <= 2; i++) {
            float y = h - (i * (h / 2)); float val = (i * (max / 2));
            canvas.DrawString(val.ToString("F0"), 0, y - 5, pl - 5, 10, HorizontalAlignment.Right, VerticalAlignment.Center);
            if (i > 0) canvas.DrawLine(pl, y, dirtyRect.Width, y);
        }

        float stepX = w / (Values.Length - 1);
        var path = new PathF(); path.MoveTo(pl, h - (Values[0] / max * h));
        for (int i = 1; i < Values.Length; i++) path.LineTo(pl + i * stepX, h - (Values[i] / max * h));
        
        var fill = new PathF(path); fill.LineTo(dirtyRect.Width, h); fill.LineTo(pl, h); fill.Close();
        canvas.FillColor = AccentColor.WithAlpha(0.2f); canvas.FillPath(fill);
        
        canvas.StrokeSize = 2; canvas.StrokeColor = AccentColor; canvas.DrawPath(path);

        for (int i = 0; i < Labels.Length; i++) {
            float x = pl + i * stepX;
            canvas.DrawString(Labels[i], x - 15, h + 5, 30, 15, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}
