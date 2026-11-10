using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodExpiryApp.Models;

public class ShoppingItem : INotifyPropertyChanged
{
    private bool _isChecked;
    
    public string Name { get; set; } = "";
    public string Quantity { get; set; } = "";
    public string Emoji { get; set; } = "🛒";
    public double Price { get; set; } // Цена товара

    public bool IsChecked
    {
        get => _isChecked;
        set { _isChecked = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
