using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class SplashPage : ContentPage
{
    bool started;
    AbsoluteLayout _rootLayout;
    ProgressBar progress;
    string currentIcon;

    public SplashPage()
    {
        BackgroundColor = Color.FromArgb("#09090F");
        currentIcon = ThemeManager.CurrentIcon.ToLower();

        _rootLayout = new AbsoluteLayout();

        // 1. Иконка (берем актуальную из настроек)
        var iconBorder = new Border
        {
            WidthRequest = 136,
            HeightRequest = 136,
            BackgroundColor = Color.FromArgb("#15121F"),
            Stroke = Color.FromArgb("#493064"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 32 },
            Content = new Image
            {
                Source = GetIconSource(currentIcon),
                Aspect = Aspect.AspectFit,
                Margin = 20
            }
        };
        AbsoluteLayout.SetLayoutBounds(iconBorder, new Rect(0.5, 0.4, 136, 136));
        AbsoluteLayout.SetLayoutFlags(iconBorder, AbsoluteLayoutFlags.PositionProportional);

        // 2. Текст
        var title = new Label
        {
            Text = "FoodExpiryApp",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center
        };
        AbsoluteLayout.SetLayoutBounds(title, new Rect(0.5, 0.55, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(title, AbsoluteLayoutFlags.PositionProportional);

        var subtitle = new Label
        {
            Text = "Свежесть под контролем",
            FontSize = 15,
            TextColor = Color.FromArgb("#AAA2BD"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        AbsoluteLayout.SetLayoutBounds(subtitle, new Rect(0.5, 0.62, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(subtitle, AbsoluteLayoutFlags.PositionProportional);

        // 3. Прогресс-бар
        progress = new ProgressBar
        {
            Progress = 0,
            ProgressColor = GetProgressBarColor(currentIcon),
            BackgroundColor = Color.FromArgb("#30263F"),
            HeightRequest = 8
        };
        AbsoluteLayout.SetLayoutBounds(progress, new Rect(0.5, 0.8, 250, 8));
        AbsoluteLayout.SetLayoutFlags(progress, AbsoluteLayoutFlags.PositionProportional);

        var loading = new Label
        {
            Text = "Загрузка приложения…",
            FontSize = 13,
            TextColor = Color.FromArgb("#AAA2BD"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        AbsoluteLayout.SetLayoutBounds(loading, new Rect(0.5, 0.85, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(loading, AbsoluteLayoutFlags.PositionProportional);

        // Добавляем элементы на фон
        _rootLayout.Children.Add(iconBorder);
        _rootLayout.Children.Add(title);
        _rootLayout.Children.Add(subtitle);
        _rootLayout.Children.Add(progress);
        _rootLayout.Children.Add(loading);

        // Применяем спецэффекты
        ApplyThemeEffects();

        Content = _rootLayout;
    }

    private string GetIconSource(string icon)
    {
        return icon switch
        {
            "winter" => "appicon_winter_food_qr.png",
            "halloween" => "appicon_halloween_food_qr.png",
            "mono" => "appicon_monochrome_food_qr.png",
            "neon" => "appicon_neon_food_qr.png",
            _ => "appicon_classic_food_qr.png"
        };
    }

    private Color GetProgressBarColor(string icon)
    {
        return icon switch
        {
            "winter" => Color.FromArgb("#87CEEB"),
            "mono" => Colors.White,
            "halloween" => Color.FromArgb("#FF7518"),
            "neon" => Color.FromArgb("#A978F5"),
            _ => Color.FromArgb("#A978F5")
        };
    }

    private void ApplyThemeEffects()
    {
        switch (currentIcon)
        {
            case "winter":
                StartSnowAnimation();
                break;
            case "halloween":
                AddCobwebs();
                break;
            case "neon":
                StartNeonRGBAnimation();
                break;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (started) return;
        started = true;

        try
        {
            await progress.ProgressTo(0.35, 400, Easing.CubicOut);
            await progress.ProgressTo(0.70, 500, Easing.CubicInOut);
            await progress.ProgressTo(1.0, 400, Easing.CubicOut);
            await Task.Delay(250);

            Application.Current!.MainPage = new NavigationPage(new MainPage(new ProductRepository()));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка запуска", ex.Message, "OK");
        }
    }

    // --- АНИМАЦИИ ---

    private void StartSnowAnimation()
    {
        _ = Task.Run(async () =>
        {
            while (started)
            {
                await Dispatcher.DispatchAsync(() =>
                {
                    var snowflake = new Label
                    {
                        Text = "❄",
                        TextColor = Colors.White,
                        FontSize = Random.Shared.Next(15, 30),
                        Opacity = 0.7
                    };
                    
                    double startX = Random.Shared.NextDouble() * (Width > 0 ? Width : 400);
                    AbsoluteLayout.SetLayoutBounds(snowflake, new Rect(startX, -50, 30, 30));
                    _rootLayout.Children.Add(snowflake);

                    // Анимация падения
                    var targetY = Height > 0 ? Height + 50 : 800;
                    snowflake.TranslateTo(startX + Random.Shared.Next(-50, 50), targetY, 3000, Easing.Linear)
                             .ContinueWith(_ => Dispatcher.Dispatch(() => _rootLayout.Children.Remove(snowflake)));
                });
                await Task.Delay(300);
            }
        });
    }

    private void AddCobwebs()
    {
        var corners = new[] { (0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0) };
        foreach (var (x, y) in corners)
        {
            var web = new Label { Text = "🕸️", FontSize = 90, Opacity = 0.2 };
            AbsoluteLayout.SetLayoutBounds(web, new Rect(x, y, 100, 100));
            AbsoluteLayout.SetLayoutFlags(web, AbsoluteLayoutFlags.PositionProportional);
            _rootLayout.Children.Add(web);
        }
    }

    private void StartNeonRGBAnimation()
    {
        var colors = new[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Cyan, Colors.Blue, Colors.Purple };
        int index = 0;
        
        _ = Task.Run(async () =>
        {
            while (started)
            {
                await Dispatcher.DispatchAsync(() =>
                {
                    progress.ProgressColor = colors[index % colors.Length];
                });
                index++;
                await Task.Delay(200);
            }
        });
    }
}
