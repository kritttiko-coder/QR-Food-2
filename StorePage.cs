using Microsoft.Maui.Controls;

namespace FoodExpiryApp;

public class StorePage : ContentPage
{
    public StorePage(ProductRepository repo)
    {
        Title = "Магазин";
        ThemeManager.BindPage(this);

        var title = new Label { Text = "Магазин", FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(title);

        var grid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        grid.Add(title, 0, 0);
        grid.Add(new BottomNavBar(1, repo), 0, 1);

        // Свайпы
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => BottomNavBar.SwitchToTab(2, repo); // Влево -> Аккаунт
        grid.GestureRecognizers.Add(swipeLeft);

        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => BottomNavBar.SwitchToTab(0, repo); // Вправо -> Главная
        grid.GestureRecognizers.Add(swipeRight);

        Content = grid;
    }
}
