using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoodExpiryApp;

// Класс для рисования линейного графика
public class LineChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public Color AccentColor { get; set; } = Colors.Purple;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length < 2) return;

        canvas.StrokeSize = 3;
        canvas.StrokeColor = AccentColor;
        canvas.StrokeLineJoin = LineJoin.Round;

        float maxVal = Values.Max();
        if (maxVal == 0) maxVal = 1;

        float stepX = dirtyRect.Width / (Values.Length - 1);
        var path = new PathF();
        path.MoveTo(0, dirtyRect.Height - (Values[0] / maxVal * dirtyRect.Height));

        for (int i = 1; i < Values.Length; i++)
        {
            float x = i * stepX;
            float y = dirtyRect.Height - (Values[i] / maxVal * dirtyRect.Height);
            path.LineTo(x, y);
        }

        // Заливка под графиком (полупрозрачная)
        var fillPath = new PathF(path);
        fillPath.LineTo(dirtyRect.Width, dirtyRect.Height);
        fillPath.LineTo(0, dirtyRect.Height);
        fillPath.Close();
        
        canvas.FillColor = AccentColor.WithAlpha(0.2f);
        canvas.FillPath(fillPath);

        // Сама линия
        canvas.DrawPath(path);

        // Точки на графике
        canvas.FillColor = Colors.White;
        for (int i = 0; i < Values.Length; i++)
        {
            float x = i * stepX;
            float y = dirtyRect.Height - (Values[i] / maxVal * dirtyRect.Height);
            canvas.FillCircle(x, y, 4);
        }
    }
}

// Класс для рисования круговой диаграммы
public class DonutChartDrawable : IDrawable
{
    public float[] Values { get; set; } = Array.Empty<float>();
    public Color[] Colors { get; set; } = Array.Empty<Color>();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values == null || Values.Length == 0) return;

        float total = Values.Sum();
        if (total == 0) return;

        float startAngle = 0;
        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2;
        float innerRadius = radius * 0.6f; // Делаем "дырку" в центре

        for (int i = 0; i < Values.Length; i++)
        {
            float sweepAngle = (Values[i] / total) * 360;
            canvas.FillColor = Colors[i % Colors.Length];
            
            // Рисуем сектор
            var path = new PathF();
            path.MoveTo(centerX, centerY);
            path.AddArc(centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sweepAngle, false);
            path.Close();
            canvas.FillPath(path);

            startAngle += sweepAngle;
        }

        // Вырезаем середину, чтобы получилось кольцо
        canvas.FillColor = Color.FromArgb("#1C1C22"); // Цвет фона карточки
        canvas.FillCircle(centerX, centerY, innerRadius);
    }
}
