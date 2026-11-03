using Microsoft.Maui.Controls.Shapes;
namespace FoodExpiryApp;
public class InfoPage:ContentPage
{ public InfoPage()
{Title="Инфо";BackgroundColor=Color.FromArgb("#111116");
var s=new VerticalStackLayout{Padding=22,Spacing=10};
s.Children.Add(new Label{Text="Цвета сроков годности",FontSize=26,TextColor=Colors.White,FontAttributes=FontAttributes.Bold});
Add(s,"Просрочен","#B00035");Add(s,"0–3 дня","#FF414D");Add(s,"4–7 дней","#FF962E");Add(s,"8–14 дней","#FF962E");Add(s,"15–21 день","#E8C928");Add(s,"22+ дня","#39C879");
s.Children.Add(new Label{Text="Фоны категорий: молочные — белый; напитки — голубой; мясо/рыба — розово-красный; овощи/фрукты — зелёный; заморозка — синий; прочее — фиолетовый.",TextColor=Color.FromArgb("#B7B2C1")});
s.Children.Add(new Label{Text="Цвет — визуальная подсказка, а не оценка безопасности пищи.",TextColor=Color.FromArgb("#FFB65C")});
