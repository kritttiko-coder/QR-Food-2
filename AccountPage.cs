using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class AccountPage : ContentPage
{
    private readonly ProductRepository _repo;
    private string _userName = "Andry";
    private ImageSource? _userAvatar;

    public AccountPage(ProductRepository repo)
    {
        _repo = repo;
        Title = Loc.T("Account");
        ThemeManager.BindPage(this);
        LoadUserData();
        BuildUI();
    }

    private void LoadUserData()
    {
        _userName = Preferences.Default.Get("user_name", "Andry");
        var avatarPath = Preferences.Default.Get("user_avatar", "");
        if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath)) _userAvatar = ImageSource.FromFile(avatarPath);
    }

    private void BuildUI()
    {
        var content = new VerticalStackLayout { Padding = 16, Spacing = 16 };

        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var title = new Label { Text = Loc.T("Account"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);
        var gear = new Image { Source = "icon_gear.png", WidthRequest = 24, HeightRequest = 24 };
        gear.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        gear.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new SettingsPage(_repo))) });
        header.Add(title, 0, 0); header.Add(gear, 1, 0);
        content.Children.Add(header);

        // Профиль (Кликабельный)
        var profileBorder = CreateCard();
        var profileGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        
        var avatar = new Border { WidthRequest = 60, HeightRequest = 60, StrokeShape = new RoundRectangle { CornerRadius = 30 }, Stroke = ThemeManager.AccentColor, StrokeThickness = 2, BackgroundColor = Color.FromArgb("#2A2A35"), Content = new Image { Source = _userAvatar ?? "icon_account.png", WidthRequest = 32, HeightRequest = 32 } };
        var avatarTap = new TapGestureRecognizer();
        avatarTap.Tapped += async (_, _) =>
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, "user_avatar.jpg");
                await using (var src = await photo.OpenReadAsync()) await using (var dst = File.Create(path)) await src.CopyToAsync(dst);
                Preferences.Default.Set("user_avatar", path);
                _userAvatar = ImageSource.FromFile(path);
                BuildUI(); // Перерисовка
            }
        };
        avatar.GestureRecognizers.Add(avatarTap);

        var nameStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var uName = new Label { Text = _userName, FontSize = 18, FontAttributes = FontAttributes.Bold };
        var uRole = new Label { Text = Loc.T("UserRole"), FontSize = 12, TextColor = Colors.Gray };
        ThemeManager.BindText(uName);
        nameStack.Children.Add(uName); nameStack.Children.Add(uRole);
        
        var nameTap = new TapGestureRecognizer();
        nameTap.Tapped += async (_, _) =>
        {
            string result = await DisplayPromptAsync("Имя", "Введите ваше имя:", "OK", "Отмена", initialValue: _userName);
            if (!string.IsNullOrWhiteSpace(result)) { _userName = result; Preferences.Default.Set("user_name", result); BuildUI(); }
        };
        nameStack.GestureRecognizers.Add(nameTap);

        var pChevron = new Label { Text = ">", FontSize = 18, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
        profileGrid.Add(avatar, 0, 0); profileGrid.Add(nameStack, 1, 0); profileGrid.Add(pChevron, 2, 0);
        profileBorder.Content = profileGrid;
        content.Children.Add(profileBorder);

        // Синхронизация
        var syncBorder = CreateCard();
        var syncGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        var syncIcon = new Image { Source = "icon_sync.png", WidthRequest = 24, HeightRequest = 24 };
        syncIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var syncStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var sTitle = new Label { Text = Loc.T("Sync"), FontSize = 14, FontAttributes = FontAttributes.Bold };
        var sDesc = new Label { Text = Loc.T("LastSync"), FontSize = 11, TextColor = Colors.Gray };
        ThemeManager.BindText(sTitle);
        syncStack.Children.Add(sTitle); syncStack.Children.Add(sDesc);
        var check = new Label { Text = "✔", TextColor = Color.FromArgb("#55A630"), FontSize = 20, VerticalOptions = LayoutOptions.Center };
        syncGrid.Add(syncIcon, 0, 0); syncGrid.Add(syncStack, 1, 0); syncGrid.Add(check, 2, 0);
        syncBorder.Content = syncGrid;
        content.Children.Add(syncBorder);

        // Меню
        content.Children.Add(new Label { Text = Loc.T("Profile"), FontSize = 16, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10, 0, 0) });
        var profileList = new VerticalStackLayout { Spacing = 1 };
        profileList.Children.Add(CreateListItem("icon_account.png", Loc.T("ChangeName"), _userName));
        profileList.Children.Add(CreateListItem("icon_brush.png", Loc.T("Appearance"), Loc.T("AppearanceDesc"), async () => await Navigation.PushAsync(new CustomizationPage())));
        var profileWrap = CreateCard(); profileWrap.Content = profileList; profileWrap.Padding = 0;
        content.Children.Add(profileWrap);

        content.Children.Add(new Label { Text = Loc.T("More"), FontSize = 16, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10, 0, 0) });
        var moreList = new VerticalStackLayout { Spacing = 1 };
        moreList.Children.Add(CreateListItem("icon_coffee.png", Loc.T("SupportDev"), null, async () => await Navigation.PushAsync(new SupportPage())));
        moreList.Children.Add(CreateListItem("icon_help.png", Loc.T("Help"), null, async () => await Navigation.PushAsync(new HelpPage())));
        moreList.Children.Add(CreateListItem("icon_info.png", Loc.T("AboutApp"), "v0.2.0", async () => await Navigation.PushAsync(new InfoPage())));
        var moreWrap = CreateCard(); moreWrap.Content = moreList; moreWrap.Padding = 0;
        content.Children.Add(moreWrap);

        var logoutBorder = new Border { Stroke = ThemeManager.AccentColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 14 }, HeightRequest = 50, Margin = new Thickness(0, 20, 0, 0), BackgroundColor = Colors.Transparent };
        var logoutStack = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        logoutStack.Children.Add(new Image { Source = "icon_logout.png", WidthRequest = 20, HeightRequest = 20 }.BindTint(ThemeManager.AccentColor));
        logoutStack.Children.Add(new Label { Text = Loc.T("LogOut"), TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 });
        logoutBorder.Content = logoutStack;
        logoutBorder.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await DisplayAlert("Выход", "Вы уверены?", "Да", "Отмена")) });
        content.Children.Add(logoutBorder);

        var scroll = new ScrollView { Content = content };
        var rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        rootGrid.Add(scroll, 0, 0);
        rootGrid.Add(new BottomNavBar(4, _repo), 0, 1);

        Content = rootGrid;
        ThemeManager.Apply();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (Content == null) return;
        Content.GestureRecognizers.Clear();
        Content.GestureRecognizers.Add(new SwipeGestureRecognizer { Direction = SwipeDirection.Left, Command = new Command(() => BottomNavBar.SwitchToTab(0, _repo)) });
        Content.GestureRecognizers.Add(new SwipeGestureRecognizer { Direction = SwipeDirection.Right, Command = new Command(() => BottomNavBar.SwitchToTab(3, _repo)) });
    }

    private Border CreateCard() { var b = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 16 }; b.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface); b.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border); return b; }
    private View CreateListItem(string icon, string title, string? value, Action? onTap = null)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12, Padding = new Thickness(16, 12) };
        grid.Add(new Image { Source = icon, WidthRequest = 22, HeightRequest = 22 }.BindTint(ThemeManager.AccentColor), 0, 0);
        grid.Add(new Label { Text = title, FontSize = 14, VerticalOptions = LayoutOptions.Center }.BindText(), 1, 0);
        var rightStack = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        if (!string.IsNullOrEmpty(value)) rightStack.Children.Add(new Label { Text = value, FontSize = 12, TextColor = Colors.Gray });
        rightStack.Children.Add(new Label { Text = ">", FontSize = 16, TextColor = Colors.Gray });
        grid.Add(rightStack, 2, 0);
        if (onTap != null) grid.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return grid;
    }
}
