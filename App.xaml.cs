namespace FoodExpiryApp; public partial class App : Application { public App(){ InitializeComponent(); MainPage = new NavigationPage(new MainPage(new ProductRepository())); } }
