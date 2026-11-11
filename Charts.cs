using Microsoft.Maui.Graphics;
using System;
using System.Linq;

namespace FoodExpiryApp;

public class LineChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public string[] Labels { get; set; } = Array.Empty<string>();
    public Color AccentColor { get; set; } = Colors.Purple;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length < 2) return;

        float paddingLeft = 25;
        float paddingBottom = 20;
        float chartWidth = dirtyRect.Width - paddingLeft;
        float chartHeight = dirtyRect.Height - paddingBottom;

        float maxVal = Values.Max();
        if (maxVal == 0) maxVal = 1;
        maxVal = (float)(Math.Ceiling(maxVal / 10.0) * 10);
        if (maxVal == 0) maxVal = 10;

        canvas.FontSize = 9;
        canvas.FontColor = Colors.Gray;
        canvas.StrokeSize = 0.5f;
        canvas.StrokeColor = Colors.Gray.WithAlpha(0.2f);

        int ySteps = 3;
        for (int i = 0; i <= ySteps; i++)
        {
            float y = chartHeight - (i * (chartHeight / ySteps));
            float valY = (i * (maxVal / ySteps));
            canvas.DrawLine(paddingLeft, y, dirtyRect.Width, y);
            canvas.DrawString(valY.ToString("F0"), 0, y - 5, paddingLeft - 5, 10, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        float stepX = chartWidth / (Values.Length - 1);
        var path = new PathF();
        path.MoveTo(paddingLeft, chartHeight - (Values[0] / maxVal * chartHeight));

        for (int i = 1; i < Values.Length; i++)
        {
            float x = paddingLeft + (i * stepX);
            float y = chartHeight - (Values[i] / maxVal * chartHeight);
            path.LineTo(x, y);
        }

        var fillPath = new PathF(path);
        fillPath.LineTo(dirtyRect.Width, chartHeight);
        fillPath.LineTo(paddingLeft, chartHeight);
        fillPath.Close();
        
        canvas.FillColor = AccentColor.WithAlpha(0.2f);
        canvas.FillPath(fillPath);

        canvas.StrokeSize = 3;
        canvas.StrokeColor = AccentColor;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(path);

        canvas.FillColor = Colors.White;
        for (int i = 0; i < Values.Length; i++)
        {
            float x = paddingLeft + (i * stepX);
            float y = chartHeight - (Values[i] / maxVal * chartHeight);
            canvas.FillCircle(x, y, 4);

            if (i < Labels.Length)
            {
                canvas.FontColor = Colors.Gray;
                canvas.DrawString(Labels[i], x - 15, chartHeight + 5, 30, 15, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
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
        float total = Values.Sum();
        if (total == 0) return;

        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2.2f;
        float innerRadius = radius * 0.65f;

        float startAngle = -90;

        for (int i = 0; i < Values.Length; i++)
        {
            float sweepAngle = (Values[i] / total) * 360;
            canvas.FillColor = Colors[i % Colors.Length];
            
            var path = new PathF();
            path.MoveTo(centerX, centerY);
            path.AddArc(centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, startAngle + sweepAngle, true);
            path.Close();
            canvas.FillPath(path);

            startAngle += sweepAngle;
        }

        canvas.FillColor = BackgroundColor; 
        canvas.FillCircle(centerX, centerY, innerRadius);
    }
}

public class CostChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public Color AccentColor { get; set; } = Colors.Purple;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length < 2) return;
        
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        float max = Values.Max();
        if (max == 0) max = 1;

        var path = new PathF();
        float stepX = w / (Values.Length - 1);
        path.MoveTo(0, h - (Values[0] / max * h));

        for (int i = 1; i < Values.Length; i++)
        {
            float x = i * stepX;
            float y = h - (Values[i] / max * h);
            path.LineTo(x, y);
        }

        var fillPath = new PathF(path);
        fillPath.LineTo(w, h);
        fillPath.LineTo(0, h);
        fillPath.Close();

        canvas.SetFillPaint(new LinearGradientPaint
        {
            StartColor = AccentColor.WithAlpha(0.4f),
            EndColor = Colors.Transparent,
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1)
        }, dirtyRect);
        canvas.FillPath(fillPath);

        canvas.StrokeColor = AccentColor;
        canvas.StrokeSize = 3;
        canvas.DrawPath(path);
    }
}
