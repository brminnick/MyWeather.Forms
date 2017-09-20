using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

using Plugin.Geolocator;
using Plugin.TextToSpeech;

using MyWeather.Models;
using MyWeather.Helpers;
using MyWeather.Services;

namespace MyWeather.ViewModels
{
    public class WeatherViewModel : BaseViewModel
    {
        #region Constant Fields
        const string _errorMessage = "Unable to get Weather";
        #endregion

        #region Fields
        bool _useGPS, _isBusy;
        string _temp = string.Empty, _condition = string.Empty;
        WeatherForecastRoot _forecast;
        ICommand _getWeather, _crashButtonTapped, _feedbackButtonTapped;
        #endregion

        #region Properties
        public ICommand GetWeatherCommand => _getWeather ??
            (_getWeather = new Command(async () => await ExecuteGetWeatherCommand()));

        public ICommand CrashButtonTapped => _crashButtonTapped ??
            (_crashButtonTapped = new Command(ExecuteCrashButtonCommand));

        public ICommand FeedbackButtonTapped => _feedbackButtonTapped ??
            (_feedbackButtonTapped = new Command(ExecuteFeedbackButtonCommand));

        public string Location
        {
            get => Settings.City;
            set
            {
                Settings.City = value;
                OnPropertyChanged();
            }
        }

        public bool UseGPS
        {
            get => _useGPS;
            set
            {
                HockeyappHelpers.TrackEvent(HockeyappConstants.GPSSwitchToggled,
                    new Dictionary<string, string> { { "Use GPS Value", value.ToString() } },
                    null);

                SetProperty(ref _useGPS, value);
            }
        }

        public bool IsImperial
        {
            get => Settings.IsImperial;
            set
            {
                Settings.IsImperial = value;
                OnPropertyChanged();
            }
        }

        public string Temp
        {
            get => _temp;
            set => SetProperty(ref _temp, value);
        }

        public string Condition
        {
            get => _condition;
            set => SetProperty(ref _condition, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        public WeatherForecastRoot Forecast
        {
            get => _forecast;
            set => SetProperty(ref _forecast, value);
        }
        #endregion

        void ExecuteCrashButtonCommand()
        {
            HockeyappHelpers.TrackEvent(HockeyappConstants.CrashButtonTapped);
            throw new Exception(HockeyappConstants.CrashButtonTapped);
        }

        void ExecuteFeedbackButtonCommand()
        {
            HockeyappHelpers.TrackEvent(HockeyappConstants.FeedbackButtonTapped);
            DependencyService.Get<IHockeyappFeedbackService>()?.GiveFeedback();
        }

        async Task ExecuteGetWeatherCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                WeatherRoot weatherRoot = null;
                var units = IsImperial ? Units.Imperial : Units.Metric;


                if (UseGPS)
                {
                    var gps = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(10));
                    weatherRoot = await WeatherService.GetWeather(gps.Latitude, gps.Longitude, units);
                }
                else
                {
                    //Get weather by city
                    weatherRoot = await WeatherService.GetWeather(Location.Trim(), units);
                }


                //Get forecast based on cityId
                Forecast = await WeatherService.GetForecast(weatherRoot.CityId, units);

                var unit = IsImperial ? "F" : "C";
                Temp = $"Temp: {weatherRoot?.MainWeather?.Temperature ?? 0}°{unit}";
                Condition = $"{weatherRoot?.Name}: {weatherRoot?.Weather?[0]?.Description ?? string.Empty}";
                await CrossTextToSpeech.Current.Speak(Temp + " " + Condition);
            }
            catch (Exception ex)
            {
                Temp = _errorMessage;
                HockeyappHelpers.Report(ex);
            }
            finally
            {
                IsBusy = false;
                TrackGetWeatherEvent();
            }
        }

        void TrackGetWeatherEvent()
        {
            var eventDictionaryHockeyApp = new Dictionary<string, string>
            {
                {"Use GPS Enabled", UseGPS.ToString()}
            };

            try
            {
                if (!Temp.Contains(_errorMessage))
                {

                    var locationCityName = UseGPS
                        ? Condition?.Substring(0, Condition.IndexOf(":", StringComparison.Ordinal))
                        : Location?.Substring(0, Location.IndexOf(",", StringComparison.Ordinal));

                    eventDictionaryHockeyApp.Add("Location", locationCityName);
                }
            }
            catch (Exception ex)
            {
                HockeyappHelpers.Report(ex);
            }
            finally
            {
                HockeyappHelpers.TrackEvent(HockeyappConstants.GetWeatherButtonTapped, eventDictionaryHockeyApp, null);
            }
        }
    }
}
