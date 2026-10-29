using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class SplashPage : ContentPage
{
    bool started;

    readonly ProgressBar progress = new()
    {
        Progress = 0,
        ProgressColor = Color.FromArgb("#A978F5"),
        BackgroundColor = Color.FromArgb("#30263F"),
        HeightRequest = 8
    };

    public SplashPage()
    {
        BackgroundColor = Color.FromArgb("#09090F");

        var logo = new Border
        {
            WidthRequest = 136,
            HeightRequest = 136,
            BackgroundColor = Color.FromArgb("#15121F"),
            Stroke = Color.FromArgb("#493064"),
            StrokeThickness = 2,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 32
            },
            Content = new Label
            {
                Text = "▦",
                FontSize = 90,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A978F5"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var title = new Label
        {
            Text = "FoodExpiryApp",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var subtitle = new Label
        {
            Text = "Свежесть под контролем",
            FontSize = 15,
            TextColor = Color.FromArgb("#AAA2BD"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var loading = new Label
        {
            Text = "Загрузка приложения…",
            FontSize = 13,
            TextColor = Color.FromArgb("#AAA2BD"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var center = new VerticalStackLayout
        {
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { logo, title, subtitle }
        };

        var bottom = new VerticalStackLayout
        {
            Spacing = 12,
            Margin = new Thickness(36, 0),
            Children = { progress, loading }
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children = { center, bottom }
        };

        Grid.SetRow(bottom, 1);
        bottom.Margin = new Thickness(36, 0, 36, 44);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (started)
            return;

        started = true;

        try
        {
            await progress.ProgressTo(0.35, 400, Easing.CubicOut);
            await progress.ProgressTo(0.70, 500, Easing.CubicInOut);
            await progress.ProgressTo(1.0, 400, Easing.CubicOut);
            await Task.Delay(250);

            Application.Current!.MainPage =
                new NavigationPage(new MainPage(new ProductRepository()));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка запуска", ex.Message, "OK");
        }
    }
}
