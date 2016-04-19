using System;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.WeatherFramework
{
    public class WeatherManager : NetworkedBehaviour
    {
        private Weather _forecast;
        private UniStormWeatherSystem_C _weatherSystem;
        public static WeatherManager Instance { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
                Channel.Send(nameof(Network_RequestWeather), ECall.Server, EPacket.UPDATE_RELIABLE_BUFFER);
            }
        }

        [NetworkCall]
        private void Network_RequestWeather(Identity ident)
        {
            if (Channel.ValidateServer(ident, false)) return;
            Channel.Send(nameof(Network_SetWeather), ident, EPacket.UPDATE_RELIABLE_BUFFER, (int)Weather);
            Channel.Send(nameof(Network_ChangeWeatherInstant), ident, EPacket.UPDATE_RELIABLE_BUFFER);
        }

        public void SendWeatherTimeUpdate(Identity target)
        {
            Channel.Send(nameof(Network_SetTime), target, EPacket.UPDATE_RELIABLE_BUFFER, _weatherSystem.startTime, World.Instance.Sun_Moon.transform.rotation.eulerAngles);
        }

        [NetworkCall]
        private void Network_SetTime(Identity ident, float time, Vector3 rot)
        {
            Channel.ValidateServer(ident);
            _weatherSystem.startTime = time;
            World.Instance.Sun_Moon.transform.rotation = Quaternion.Euler(rot);
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
                _weatherSystem.startTime += 6000;
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

            Channel.Send(nameof(Network_SetWeather), ECall.Others, EPacket.UPDATE_RELIABLE_BUFFER, (int)newWeather);
        }

        [NetworkCall]
        private void Network_SetWeather(Identity ident, int weather)
        {
            Channel.ValidateServer(ident);
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


        public void ChangeWeatherInstant()
        {
            _weatherSystem.InstantWeather();
            if (IsServer())
            {
                Channel.Send(nameof(Network_SetWeather), ECall.Others, EPacket.UPDATE_RELIABLE_BUFFER, (int)_forecast);
                Channel.Send(nameof(Network_ChangeWeatherInstant), ECall.Others, EPacket.UPDATE_RELIABLE_BUFFER);
            }
        }

        [NetworkCall]
        private void Network_ChangeWeatherInstant(Identity ident)
        {
            Channel.ValidateServer(ident);
            ChangeWeatherInstant();
        }

        public Weather GetRandomWeather()
        {
            Array weatherValues = Enum.GetValues(typeof(Weather));
            System.Random random = new System.Random();
            return (Weather)weatherValues.GetValue(random.Next(weatherValues.Length));
        }
    }
}