using System;
using Artngame.SKYMASTER;
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
        public static SkyMaster SkyMaster => World.Instance.WeatherSettings.SkyMaster;
        private SkyMasterManager.Volume_Weather_types _cachedWeather;
        protected override void Awake()
        {
            base.Awake();

            if (!Connection.IsSinglePlayer && !IsServer())
            {
                Channel.Send(nameof(Network_RequestWeather), ECall.Server);
            }

            _cachedWeather = Weather;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER)]
        private void Network_RequestWeather(Identity ident)
        {
            Channel.Send(nameof(Network_SetWeather), ident, (int)Weather);
        }

        public void SendWeatherTimeUpdate()
        {
            if (!IsServer()) return;
            Channel.Send(nameof(Network_SetTime), ECall.Others, SkyMaster.SkyManager.Current_Time);
        }

        public void SendWeatherTimeUpdate(Identity target, float time = -1f)
        {
            if (!IsServer()) return;
            if (time < 0) time = SkyMaster.SkyManager.Current_Time;
            Channel.Send(nameof(Network_SetTime), target, time);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetTime(Identity ident, float time)
        {
            SetDayTime(time);
        }

        protected override void Update()
        {
            base.Update();
            if (Debug.isDebugBuild)
            {
                if (Input.GetKeyDown(KeyCode.F1) && IsServer())
                {
                    Weather = GetRandomWeather();
                }
                if (Input.GetKey(KeyCode.F2) && IsServer())
                {
                    SendWeatherTimeUpdate();
                    World.Instance.WeatherSettings.SkyMaster.SkyManager.Current_Time += 0.05f;
                }
            }

            if (Weather != _cachedWeather)
            {
                OnWeatherChange(_cachedWeather, Weather);
                _cachedWeather = Weather;
            }
        }

        public void SetDayTime(float time)
        {
            SkyMaster.SkyManager.Current_Time = time;

            if (IsServer())
            {
                SendWeatherTimeUpdate();
            }
        }

        private void OnWeatherChange(SkyMasterManager.Volume_Weather_types currentWeather, SkyMasterManager.Volume_Weather_types newWeather)
        {
            LogUtils.Debug("Weather has changed to: " + newWeather);

            if (IsServer())
                Channel.Send(nameof(Network_SetWeather), ECall.Others, (int)newWeather);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetWeather(Identity ident, int weather)
        {
            Weather = (SkyMasterManager.Volume_Weather_types)weather;
        }

        public SkyMasterManager.Volume_Weather_types Weather
        {
            get
            {
                return
                        SkyMaster.SkyManager.currentWeatherName;
            }
            set
            {
                SkyMaster.SkyManager.currentWeatherName = value;
            }
        }

        public SkyMasterManager.Volume_Weather_types GetRandomWeather()
        {
            Array weatherValues = Enum.GetValues(typeof(SkyMasterManager.Volume_Weather_types));
            System.Random random = new System.Random();
            var value = (SkyMasterManager.Volume_Weather_types)weatherValues.GetValue(random.Next(weatherValues.Length));
            if (value == SkyMasterManager.Volume_Weather_types.FreezeStorm ||
                value == SkyMasterManager.Volume_Weather_types.VolcanoErupt ||
                value == SkyMasterManager.Volume_Weather_types.SnowStorm)
            {
                return GetRandomWeather();
            }

            return value;
        }
    }
}