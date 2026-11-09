using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class InfoPage : ContentPage
{
    public InfoPage()
    {
        // Переводим заголовок вкладки (в словаре это "Информация")
        Title = Loc.T("Info");
        ThemeManager.BindPage(this);

        var title = new Label
        {
            Text = Loc.T("ExpiryColorsTitle"),
            FontSize = 26,
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(title);

        var content = new VerticalStackLayout
        {
            Padding = 22,
            Spacing = 10,
            Children = { title }
        };

        // Сроки годности (берем ключи из словаря)
        AddColorBlock(content, Loc.T("Expired"), "#B00035");
        AddColorBlock(content, Loc.T("Exp0_3"), "#FF414D");
        AddColorBlock(content, Loc.T("Exp4_7"), "#FF962E");
        AddColorBlock(content, Loc.T("Exp8_14"), "#FF962E");
        AddColorBlock(content, Loc.T("Exp15_21"), "#E8C928");
        AddColorBlock(content, Loc.T("Exp22Plus"), "#39C879");

        // Описание фонов категорий
        var desc = new Label
        {
            Text = Loc.T("CategoryBackgrounds"),
            Margin = new Thickness(0, 10, 0, 0),
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(desc, secondary: true);
        content.Children.Add(desc);

        // Категории (используем уже существующие ключи "Cat...")
        AddColorBlock(content, Loc.T("CatDairy"), "#B3FFFFFF");
        AddColorBlock(content, Loc.T("CatDrinks"), "#B3E0F7FA");
        AddColorBlock(content, Loc.T("CatMeat"), "#B3FFEBEE");
        AddColorBlock(content, Loc.T("CatVeg"), "#B3E8F5E9");
        AddColorBlock(content, Loc.T("CatFreezer"), "#B3E3F2FD");
        AddColorBlock(content, Loc.T("CatOther"), "#B3F3E5F5");
        
        // Предупреждение
        var warning = new Label
        {
            Text = Loc.T("ColorWarning"),
            TextColor = Color.FromArgb("#FFB65C"),
            Margin = new Thickness(0, 10, 0, 0)
        };
        content.Children.Add(warning);

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    private void AddColorBlock(VerticalStackLayout s, string text, string colorHex)
    {
        s.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb(colorHex),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = 10,
            Content = new Label
            {
                Text = text,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            }
        });
    }
}
