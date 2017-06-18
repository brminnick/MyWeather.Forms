using Xamarin.Forms;

using MyWeather.View;
using MyWeather.ViewModels;

namespace MyWeather
{
    public class App : Application
    {
        public App()
        {
            var tabbedPage = new TabbedPage
            {
                Title ="My Weather",
                BindingContext = new WeatherViewModel(),
                Children =
                {
                    new WeatherView(),
                    new ForecastView()
                }
            };
            
            MainPage = new NavigationPage(tabbedPage)
            {
                BarBackgroundColor = Color.FromHex("3498db"),
                BarTextColor = Color.White
            };
        }
    }
}

