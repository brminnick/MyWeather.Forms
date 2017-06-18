using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MyWeather.Helpers
{
    public static class Settings
    {
        #region Constant Fields
        const string _isImperialKey = "is_imperial";
        const string _useCityKey = "use_city";
        const string _cityKey = "city";
        const string _cityDefault = "Seattle,WA";
        const bool _isImperialDefault = true;
        const bool _useCityDefault = true;
        #endregion

        #region Properties
        public static bool IsImperial
        {
            get => AppSettings.GetValueOrDefault(_isImperialKey, _isImperialDefault);
            set => AppSettings.AddOrUpdateValue(_isImperialKey, value);
        }

        public static bool UseCity
        {
            get => AppSettings.GetValueOrDefault(_useCityKey, _useCityDefault);
            set => AppSettings.AddOrUpdateValue(_useCityKey, value);
        }

        public static string City
        {
            get => AppSettings.GetValueOrDefault(_cityKey, _cityDefault);
            set => AppSettings.AddOrUpdateValue(_cityKey, value);
        }

        static ISettings AppSettings => CrossSettings.Current;
        #endregion

    }
}