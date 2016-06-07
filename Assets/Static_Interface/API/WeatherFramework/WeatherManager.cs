using System;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.WeatherFramework
{
    public class WeatherManager : NetworkedSingletonBehaviour<WeatherManager>
    {
        /*
        private Weather _forecast;
        private UniStormWeatherSystem_C _weatherSystem;
        public static WeatherManager Instance { get; private set; }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            Instance = null;
        }

        //Todo: WeatherChangeEvent

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            _weatherSystem = World.Instance.Weather.GetComponentInChildren<UniStormWeatherSystem_C>();

            _forecast = (Weather) _weatherSystem.weatherForecaster;

            if (!Connection.IsSinglePlayer && !IsServer())
            {
                _weatherSystem.staticWeather = true;
                Channel.Send(nameof(Network_RequestWeather), ECall.Server);
            }
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER)]
        private void Network_RequestWeather(Identity ident)
        {
            Channel.Send(nameof(Network_SetWeather), ident, (int)Weather);
            Channel.Send(nameof(Network_SetTemperature), ident, _weatherSystem.temperature);
            Channel.Send(nameof(Network_ChangeWeatherInstant), ident);
        }
        */
        public void SendWeatherTimeUpdate(Identity target)
        {
           // Channel.Send(nameof(Network_SetTime), target,
           //     _weatherSystem.realStartTime,
           //     _weatherSystem.realStartTimeMinutes,
           //     World.Sun_Moon.transform.rotation.eulerAngles);
        }
        /*
        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetTime(Identity ident, float realStartTime, int realStartTimeMinutes, Vector3 rot)
        {
            _weatherSystem.realStartTime = realStartTime;
            _weatherSystem.realStartTimeMinutes = realStartTimeMinutes;
            _weatherSystem.LoadTime();
            World.Sun_Moon.transform.rotation = Quaternion.Euler(rot);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetTemperature(Identity ident, int temperature)
        {
            _weatherSystem.temperature = temperature;
        }


        protected override void Update()
        {
            base.Update();
            if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F1))
            {
                Weather = GetRandomWeather();
                ChangeWeatherInstant();
            }
            if (Debug.isDebugBuild && Input.GetKey(KeyCode.F2))
            {
                _weatherSystem.startTime += 0.01f;
            }
            if (_weatherSystem.weatherForecaster != (int)_forecast)
            {
                OnWeatherChange(_forecast, (Weather) _weatherSystem.weatherForecaster);
            }
        }

        private void OnWeatherChange(Weather currentWeather, Weather newWeather)
        {
            LogUtils.Debug("Weather has changed to: " + newWeather);
            _forecast = (Weather)_weatherSystem.weatherForecaster;

            if (!IsServer())
            {
                return;
            }

            Channel.Send(nameof(Network_SetWeather), ECall.Others, (int)newWeather);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetWeather(Identity ident, int weather)
        {
            Weather = (Weather)weather;
        }

        public Weather Weather
        {
            get { return (Weather) _weatherSystem.weatherForecaster; }
            set
            {
                _weatherSystem.weatherForecaster = (int) value;
            }
        }
        */
        public void ChangeWeatherInstant()
        {
            //todo skymaster
            if (IsServer())
            {
                //Channel.Send(nameof(Network_SetWeather), ECall.Others, (int)1);
                //Channel.Send(nameof(Network_ChangeWeatherInstant), ECall.Others);
            }
        }
        /*
        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_ChangeWeatherInstant(Identity ident)
        {
            ChangeWeatherInstant();
        }

        public Weather GetRandomWeather()
        {
            Array weatherValues = Enum.GetValues(typeof(Weather));
            System.Random random = new System.Random();
            return (Weather)weatherValues.GetValue(random.Next(weatherValues.Length));
        }
    }
    */
    }
}