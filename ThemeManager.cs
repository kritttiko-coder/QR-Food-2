using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace FoodExpiryApp;

public static class ThemeManager
{
    // Единые ключи — и для чтения, и для записи
    public static string CurrentTheme
    {
        get => Preferences.Default.Get("app_theme", "dark");
        set => Preferences.Default.Set("app_theme", value);
    }

    public static string CurrentAccent
    {
        get => Preferences.Default.Get("app_accent", "#A978F5");
        set => Preferences.Default.Set("app_accent", value);
    }

    public static string CurrentIcon
    {
        get => Preferences.Default.Get("app_icon", "classic");
        set => Preferences.Default.Set("app_icon", value);
    }

    // Псевдонимы — чтобы остальной код (ThemeManager.Theme, .AccentHex, .Icon) продолжал работать
    public static string Theme     => CurrentTheme;
    public static string AccentHex => CurrentAccent;
    public static string Icon      => CurrentIcon;

    public const string Background    = "AppBackground";
    public const string Surface       = "AppSurface";
    public const string SurfaceAlt    = "AppSurfaceAlt";
    public const string Text          = "AppText";
    public const string SecondaryText = "AppSecondaryText";
    public const string Field         = "AppField";
    public const string Border        = "AppBorder";
    public const string Accent        = "AppAccent";
    public const string AccentText    = "AppAccentText";

    public static Color AccentColor => Color.FromArgb(CurrentAccent);

    static Color C(string hex) => Color.FromArgb(hex);

    public static void Apply()
    {
        var app = Application.Current;
        if (app == null) return;

        bool light = CurrentTheme == "light";

        Color background, surface, surfaceAlt, text, secondary, field, border;

        switch (CurrentTheme)
        {
            case "black":
                background = C("#050507"); surface = C("#101014");
                surfaceAlt = C("#18181F"); text = C("#FFFFFF");
                secondary  = C("#AAA7B2"); field = C("#14141A");
                border     = C("#292832");
                break;

            case "light":
                background = C("#F3F1F7"); surface = C("#FFFFFF");
                surfaceAlt = C("#EAE7F0"); text = C("#19171F");
                secondary  = C("#716D79"); field = C("#E9E6EF");
                border     = C("#D8D3E0");
                break;

            default: // dark
                background = C("#111116"); surface = C("#201D28");
                surfaceAlt = C("#292431"); text = C("#FFFFFF");
                secondary  = C("#B7B2C1"); field = C("#191720");
                border     = C("#383242");
                break;
        }

        var resources = app.Resources;
        resources[Background]    = background;
        resources[Surface]       = surface;
        resources[SurfaceAlt]    = surfaceAlt;
        resources[Text]          = text;
        resources[SecondaryText] = secondary;
        resources[Field]         = field;
        resources[Border]        = border;
        resources[Accent]        = AccentColor;
        resources[AccentText]    = light ? Colors.White : C("#17121F");

        app.UserAppTheme = light ? AppTheme.Light : AppTheme.Dark;
    }

    public static void SetTheme(string value)
    {
        CurrentTheme = value;
        Apply();
    }

    public static void SetAccent(string hex)
    {
        CurrentAccent = hex;
        Apply();
    }

    public static void SetIcon(string value)
    {
        CurrentIcon = value;
        Apply();
    }

    // Используй эти методы для цветов элементов на страницах.
    public static void BindPage(ContentPage page)
    {
        page.SetDynamicResource(
            VisualElement.BackgroundColorProperty, Background);
    }

    public static void BindText(Label label, bool secondary = false)
    {
        label.SetDynamicResource(
            Label.TextColorProperty,
            secondary ? SecondaryText : Text);
    }

    public static void BindEntry(Entry entry)
    {
        entry.SetDynamicResource(Entry.TextColorProperty, Text);
        entry.SetDynamicResource(Entry.PlaceholderColorProperty, SecondaryText);
        entry.SetDynamicResource(VisualElement.BackgroundColorProperty, Field);
    }

    public static void BindSearch(SearchBar search)
    {
        search.SetDynamicResource(SearchBar.TextColorProperty, Text);
        search.SetDynamicResource(SearchBar.PlaceholderColorProperty, SecondaryText);
        search.SetDynamicResource(VisualElement.BackgroundColorProperty, Field);
    }

    public static void BindButton(Button button, bool isAccent = false)
    {
        button.SetDynamicResource(
            VisualElement.BackgroundColorProperty,
            isAccent ? Accent : Surface);

        button.SetDynamicResource(
            Button.TextColorProperty,
            isAccent ? AccentText : Text);
    }
}
