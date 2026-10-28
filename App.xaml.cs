namespace FoodExpiryApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            MainPage = new NavigationPage(
                new FoodExpiryApp.MainPage(
                    new ProductRepository()
                )
            );
        }
        catch (Exception ex)
        {
            MainPage = new ContentPage
            {
                BackgroundColor = Color.FromArgb("#111116"),
                Content = new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = 20,
                        Spacing = 12,
                        Children =
                        {
                            new Label
                            {
                                Text = "FoodExpiryApp: ошибка запуска",
                                FontSize = 22,
                                TextColor = Colors.White,
                                FontAttributes = FontAttributes.Bold
                            },
                            new Label
                            {
                                Text = ex.ToString(),
                                FontSize = 13,
                                TextColor = Color.FromArgb("#FF6878")
                            }
                        }
                    }
                }
            };
        }
    }
}
