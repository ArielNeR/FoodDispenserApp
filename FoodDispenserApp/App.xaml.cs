namespace FoodDispenserApp
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();
            // Envolver la MainPage en un NavigationPage para permitir la navegación
            MainPage = new NavigationPage(mainPage);
        }
    }
}
