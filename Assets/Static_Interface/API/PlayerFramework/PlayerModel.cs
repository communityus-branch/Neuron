using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PlayerModel
    {
        private static readonly List<PlayerModel> RegisteredModels = new List<PlayerModel>();
        private Plugin _plugin;
        public static void RegisterPlayerModel(Plugin plugin, PlayerModel model)
        {
            if(GetModel(model.Name) != null)
            {
                LogUtils.LogError($"RegisterPlayerModel: A model with the name \"{model.Name}\" already exists!");
                model.Name = model.Name + "_";
                RegisterPlayerModel(plugin, model);
                return;
            }

            model._plugin = plugin;
            RegisteredModels.Add(model);
        }

        internal static void OnPluginUnload(Plugin plugin)
        {
            var models = RegisteredModels;
            foreach (var model in models.Where(m => m._plugin == plugin))
            {
                RegisteredModels.Remove(model);
            }
        }

        private static PlayerModel GetModel(string name)
        {
            return RegisteredModels.FirstOrDefault(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public abstract string Name { get; protected set; }

        internal void Apply(Player player)
        {
            GameObject newModel = LoadModel();
            player.SendMessage("OnPlayerModelChange", newModel, SendMessageOptions.DontRequireReceiver);
            var container = player.transform.FindChild("PlayerContainer");
            container.parent = newModel.transform;
            player.PlayerModel = this;
        }

        protected abstract Quaternion GetCameraLocalRotation();
        protected abstract Vector3 GetCameraLocalPosition();

        protected abstract GameObject LoadModel();
    }
}