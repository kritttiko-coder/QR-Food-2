using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class AccountPage : ContentPage
{
    private readonly ProductRepository _repo;
    private string _userName = "Пользователь";
    private ImageSource? _userAvatar;
    private Grid _rootGrid = null!;

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
        _userName = Preferences.Default.Get("user_name", "Пользователь");
        var avatarPath = Preferences.Default.Get("user_avatar", "");
        if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath)) 
            _userAvatar = ImageSource.FromFile(avatarPath);
    }

    private void BuildUI()
    {
        var content = new VerticalStackLayout { Padding = 16, Spacing = 16 };

        // --- ШАПКА ---
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var title = new Label { Text = Loc.T("Account"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);
        var gear = new Image { Source = "icon_gear.png", WidthRequest = 24, HeightRequest = 24 };
        gear.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        gear.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new SettingsPage(_repo))) });
        header.Add(title, 0, 0); header.Add(gear, 1, 0);
        content.Children.Add(header);

        // --- ПРОФИЛЬ (Карточка) ---
        var profileBorder = CreateCard();
        var profileGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        
        var avatarImage = new Image { Source = _userAvatar ?? "icon_account.png", WidthRequest = 32, HeightRequest = 32 };
        avatarImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var avatar = new Border { 
            WidthRequest = 60, HeightRequest = 60, 
            StrokeShape = new RoundRectangle { CornerRadius = 30 }, 
            Stroke = ThemeManager.AccentColor, StrokeThickness = 2, 
            BackgroundColor = Color.FromArgb("#2A2A35"), 
            Content = avatarImage,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        
        var nameStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var uName = new Label { Text = _userName, FontSize = 18, FontAttributes = FontAttributes.Bold };
        var uRole = new Label { Text = Loc.T("UserRole"), FontSize = 12, TextColor = Colors.Gray };
        ThemeManager.BindText(uName);
        nameStack.Children.Add(uName); nameStack.Children.Add(uRole);
        
        var pChevron = new Label { Text = ">", FontSize = 18, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
        profileGrid.Add(avatar, 0, 0); profileGrid.Add(nameStack, 1, 0); profileGrid.Add(pChevron, 2, 0);
        profileBorder.Content = profileGrid;
        
        // Клик по всему профилю открывает редактор
        profileBorder.GestureRecognizers.Add(new TapGestureRecognizer 
        { 
            Command = new Command(async () => await ShowEditProfilePopup()) 
        });
        content.Children.Add(profileBorder);

        // --- СИНХРОНИЗАЦИЯ ---
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

        // --- ПРОФИЛЬ СЕКЦИЯ ---
        content.Children.Add(new Label { Text = Loc.T("Profile"), FontSize = 16, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10, 0, 0) });
        var profileList = new VerticalStackLayout { Spacing = 1 };
        
        // ✅ Кнопка "Изменить имя" теперь рабочая
        profileList.Children.Add(CreateListItem("icon_account.png", Loc.T("ChangeName"), _userName, async () => await ShowEditProfilePopup()));
        profileList.Children.Add(CreateListItem("icon_brush.png", Loc.T("Appearance"), Loc.T("AppearanceDesc"), async () => await Navigation.PushAsync(new CustomizationPage())));
        
        var profileWrap = CreateCard(); profileWrap.Content = profileList; profileWrap.Padding = 0;
        content.Children.Add(profileWrap);

        // --- ДОПОЛНИТЕЛЬНО СЕКЦИЯ ---
        content.Children.Add(new Label { Text = Loc.T("More"), FontSize = 16, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10, 0, 0) });
        var moreList = new VerticalStackLayout { Spacing = 1 };
        
        // ✅ Новая кнопка "Сменить карту"
        string currentMap = Preferences.Default.Get("preferred_map", "Google");
        moreList.Children.Add(CreateListItem("icon_map.png", "Сменить карту", currentMap, async () => await ShowMapSelectionPopup()));
        
        moreList.Children.Add(CreateListItem("icon_coffee.png", Loc.T("SupportDev"), null, async () => await Navigation.PushAsync(new SupportPage())));
        moreList.Children.Add(CreateListItem("icon_help.png", Loc.T("Help"), null, async () => await Navigation.PushAsync(new HelpPage())));
        moreList.Children.Add(CreateListItem("icon_info.png", Loc.T("AboutApp"), "v0.2.0", async () => await Navigation.PushAsync(new InfoPage())));
        
        var moreWrap = CreateCard(); moreWrap.Content = moreList; moreWrap.Padding = 0;
        content.Children.Add(moreWrap);

        // --- КНОПКА ВЫЙТИ ---
        var logoutBorder = new Border { Stroke = ThemeManager.AccentColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 14 }, HeightRequest = 50, Margin = new Thickness(0, 20, 0, 0), BackgroundColor = Colors.Transparent };
        var logoutStack = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var logoutIcon = new Image { Source = "icon_logout.png", WidthRequest = 20, HeightRequest = 20 };
        logoutIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var logoutText = new Label { Text = Loc.T("LogOut"), TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
        logoutStack.Children.Add(logoutIcon); logoutStack.Children.Add(logoutText);
        logoutBorder.Content = logoutStack;
        
        logoutBorder.GestureRecognizers.Add(new TapGestureRecognizer 
        { 
            Command = new Command(async () => 
            {
                bool confirm = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Отмена");
                if (confirm) { /* Логика выхода */ }
            }) 
        });
        content.Children.Add(logoutBorder);

        var scroll = new ScrollView { Content = content };
        
        _rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        _rootGrid.Add(scroll, 0, 0);
        _rootGrid.Add(new BottomNavBar(4, _repo), 0, 1);

        Content = _rootGrid;
        ThemeManager.Apply();
    }

    // ✅ КРАСИВОЕ ОКНО РЕДАКТИРОВАНИЯ ПРОФИЛЯ
    private async Task ShowEditProfilePopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        
        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = Color.FromArgb("#201D28"), // Жесткий цвет для стабильности
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 320
        };

        // Аватарка в поп-апе
        var avatarImg = new Image { Source = _userAvatar ?? "icon_account.png", WidthRequest = 40, HeightRequest = 40 };
        avatarImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var avatarBorder = new Border { WidthRequest = 80, HeightRequest = 80, StrokeShape = new RoundRectangle { CornerRadius = 40 }, Stroke = ThemeManager.AccentColor, StrokeThickness = 2, BackgroundColor = Color.FromArgb("#2A2A35"), Content = avatarImg, HorizontalOptions = LayoutOptions.Center };
        
        var nameEntry = new Entry { Placeholder = "Ваше имя", Text = _userName, HeightRequest = 50 };
        ThemeManager.BindEntry(nameEntry);

        var btnChangeAvatar = new Border { BackgroundColor = Colors.Transparent, Stroke = ThemeManager.AccentColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "📷 Сменить аватарку", TextColor = ThemeManager.AccentColor, HorizontalOptions = LayoutOptions.Center } };
        var btnSave = new Border { BackgroundColor = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Сохранить", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center } };
        var btnCancel = new Border { BackgroundColor = Colors.Transparent, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Отмена", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center } };

        // Клик по аватарке
        btnChangeAvatar.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, "user_avatar.jpg");
                    await using (var src = await photo.OpenReadAsync()) 
                    await using (var dst = File.Create(path)) 
                        await src.CopyToAsync(dst);
                    
                    Preferences.Default.Set("user_avatar", path);
                    _userAvatar = ImageSource.FromFile(path);
                    avatarImg.Source = _userAvatar;
                }
            })
        });

        var tcs = new TaskCompletionSource<bool>();

        btnSave.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(nameEntry.Text))
                {
                    _userName = nameEntry.Text;
                    Preferences.Default.Set("user_name", _userName);
                    BuildUI(); // Обновляем интерфейс
                }
                tcs.SetResult(true);
            })
        });

        btnCancel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => tcs.SetResult(false)) });

        var btnGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        btnGrid.Add(btnCancel, 0, 0); btnGrid.Add(btnSave, 1, 0);

        popup.Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children = 
            { 
                new Label { Text = "Редактировать профиль", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.White },
                avatarBorder,
                btnChangeAvatar,
                new Label { Text = "Имя:", FontSize = 12, TextColor = Colors.Gray },
                nameEntry, 
                btnGrid 
            }
        };

        overlay.Add(popup);
        _rootGrid.Add(overlay);

        await tcs.Task;
        _rootGrid.Remove(overlay);
    }

    // ✅ КРАСИВОЕ ОКНО ВЫБОРА КАРТЫ
    private async Task ShowMapSelectionPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        
        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = Color.FromArgb("#201D28"), 
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 300
        };

        string currentMap = Preferences.Default.Get("preferred_map", "Google");

        // Функция для создания кнопки карты
        View CreateMapOption(string name, string icon)
        {
            bool isSelected = currentMap == name;
            var stack = new HorizontalStackLayout { Spacing = 12, VerticalOptions = LayoutOptions.Center };
            stack.Children.Add(new Label { Text = icon, FontSize = 24 });
            stack.Children.Add(new Label { Text = name, FontSize = 16, TextColor = isSelected ? ThemeManager.AccentColor : Colors.White, VerticalOptions = LayoutOptions.Center });
            
            if (isSelected) stack.Children.Add(new Label { Text = "✓", TextColor = ThemeManager.AccentColor, FontSize = 18, VerticalOptions = LayoutOptions.Center });
            
            var border = new Border { Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Stroke = isSelected ? ThemeManager.AccentColor : Colors.Transparent, StrokeThickness = isSelected ? 1 : 0, BackgroundColor = Color.FromArgb("#2A2A35"), Content = stack };
            
            border.GestureRecognizers.Add(new TapGestureRecognizer 
            { 
                Command = new Command(() => 
                {
                    Preferences.Default.Set("preferred_map", name);
                    // Закрываем поп-ап и перерисовываем страницу (чтобы обновить галочку)
                    BuildUI(); 
                }) 
            });
            return border;
        }

        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Children.Add(new Label { Text = "Выберите карту", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.White });
        stack.Children.Add(CreateMapOption("Google", "🌍"));
        stack.Children.Add(CreateMapOption("Яндекс", "🟡"));
        stack.Children.Add(CreateMapOption("2GIS", "🟢"));
        
        var btnClose = new Border { BackgroundColor = Colors.Transparent, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Закрыть", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center } };
        stack.Children.Add(btnClose);

        popup.Content = stack;
        overlay.Add(popup);
        _rootGrid.Add(overlay);

        var tcs = new TaskCompletionSource<bool>();
        btnClose.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => tcs.SetResult(true)) });

        await tcs.Task;
        _rootGrid.Remove(overlay);
    }

    private Border CreateCard()
    {
        var b = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 16 };
        b.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        b.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        return b;
    }

    private View CreateListItem(string icon, string title, string? value, Action? onTap = null)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12, Padding = new Thickness(16, 12) };
        
        var img = new Image { Source = icon, WidthRequest = 22, HeightRequest = 22 };
        img.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        
        var lbl = new Label { Text = title, FontSize = 14, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(lbl);
        
        var rightStack = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        if (!string.IsNullOrEmpty(value)) rightStack.Children.Add(new Label { Text = value, FontSize = 12, TextColor = Colors.Gray });
        rightStack.Children.Add(new Label { Text = ">", FontSize = 16, TextColor = Colors.Gray });
        
        grid.Add(img, 0, 0); grid.Add(lbl, 1, 0); grid.Add(rightStack, 2, 0);

        if (onTap != null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => onTap();
            grid.GestureRecognizers.Add(tap);
        }
        return grid;
    }
}
