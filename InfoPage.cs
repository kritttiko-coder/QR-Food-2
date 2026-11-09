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

        // Сроки годности 
        AddColorBlock(content, Loc.T("Expired"), "#B00035");
        AddColorBlock(content, Loc.T("Exp0_3"), "#FF414D");
        AddColorBlock(content, Loc.T("Exp4_7"), "#FF962E");
        AddColorBlock(content, Loc.T("Exp8_14"), "#FF962E");
        AddColorBlock(content, Loc.T("Exp15_21"), "#E8C928");
        AddColorBlock(content, Loc.T("Exp22Plus"), "#39C879");

        // Описание фонов категорий
        var desc = new Label { Text = Loc.T("CategoryBackgrounds"), Margin = new Thickness(0, 10, 0, 0), FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(desc, secondary: true);
        content.Children.Add(desc);

        
        AddColorBlock(content, Loc.T("CatDairy"), GetTint("Молочные"));
        AddColorBlock(content, Loc.T("CatDrinks"), GetTint("Напитки"));
        AddColorBlock(content, Loc.T("CatMeat"), GetTint("Мясо и рыба"));
        AddColorBlock(content, Loc.T("CatVeg"), GetTint("Овощи и фрукты"));
        AddColorBlock(content, Loc.T("CatFreezer"), GetTint("Заморозка"));
        AddColorBlock(content, Loc.T("CatOther"), GetTint("Прочее"));

        var warning = new Label { Text = Loc.T("ColorWarning"), TextColor = Color.FromArgb("#FFB65C"), Margin = new Thickness(0, 10, 0, 0) };
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

  private Color GetTint(string category)
   {
    var c = CategoryStyle.Tint(category);
    return Color.FromRgba(c.Red, c.Green, c.Blue, 0.45f);
   }  
   
}
