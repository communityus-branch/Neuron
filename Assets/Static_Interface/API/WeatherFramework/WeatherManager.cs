using System;
using Static_Interface.API.Utils;
using Static_Interface.Internal;
using UnityEngine;

namespace Static_Interface.API.WeatherFramework
{
    public class WeatherManager : MonoBehaviour
    {
        private Weather _currentWeather;
        private UniStormWeatherSystem_C _weatherSystem;

        //Todo: WeatherChangeEvent

        protected void Awake()
        {
            _weatherSystem = World.Instance.Weather.GetComponentInChildren<UniStormWeatherSystem_C>();
        }

        protected void Update()
        {
            if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F1))
            {
                Weather = GetRandomWeather();
            }
        }

        public Weather Weather
        {
            get { return _currentWeather; }
            set
            {
                if (_currentWeather == value) return;
                ChangeWeatherTo(value);
                _currentWeather = value;
            }
        }

        private void ChangeWeatherTo(Weather weather)
        {
            _weatherSystem.weatherForecaster = (int)weather;
            LogUtils.Log("Weather changed to: " + weather);
            ChangeWeatherInstant(); //todo: remmove this and make it smooth
        }

        public void ChangeWeatherInstant()
        {
            _weatherSystem.InstantWeather();
        }

        public Weather GetRandomWeather()
        {
            Array weatherValues = Enum.GetValues(typeof(Weather));
            System.Random random = new System.Random();
            return (Weather)weatherValues.GetValue(random.Next(weatherValues.Length));
        }
    }
}