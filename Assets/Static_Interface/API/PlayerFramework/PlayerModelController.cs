using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PlayerModelController
    {
        private static readonly List<PlayerModelController> RegisteredModels = new List<PlayerModelController>();
        private Plugin _plugin;
        public static void RegisterPlayerModel(Plugin plugin, PlayerModelController modelController)
        {
            if(plugin == null) throw new ArgumentNullException(nameof(plugin));
            RegisterPlayerModelInternal(plugin, modelController);
        }

        internal static void RegisterPlayerModelInternal(Plugin plugin, PlayerModelController modelController)
        {
            if (GetPlayerModelController(modelController.GetType()) != null)
            {
                LogUtils.LogError($"RegisterPlayerModel: The class \"{modelController.GetType().FullName}\" is already a registered controller!");
            }
            if (GetPlayerModelController(modelController.Name) != null)
            {
                LogUtils.LogError($"RegisterPlayerModel: A model with the name \"{modelController.Name}\" already exists!");
                modelController.Name = modelController.Name + "_";
                RegisterPlayerModelInternal(plugin, modelController);
                return;
            }

            modelController._plugin = plugin;
            RegisteredModels.Add(modelController);
        }

        internal static void OnPluginUnload(Plugin plugin)
        {
            var models = RegisteredModels;
            foreach (var model in models.Where(m => m._plugin == plugin))
            {
                RegisteredModels.Remove(model);
            }
        }

        public static PlayerModelController GetPlayerModelController(string name)
        {
            return RegisteredModels.FirstOrDefault(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static T GetPlayerModelController<T>() where T : PlayerModelController
        {
            return (T) GetPlayerModelController(typeof (T));
        }

        public static PlayerModelController GetPlayerModelController(Type t)
        {
            var match = RegisteredModels.FirstOrDefault(c => c.GetType() == t) ??
                        RegisteredModels.FirstOrDefault(c => c.GetType().IsInstanceOfType(t));
            return match;
        }

        public abstract string Name { get; protected set; }

        public void ApplyLocal(Player player)
        {
            ApplyInternal(player, true);
        }

        public void Apply(Player player)
        {
            ApplyInternal(player, false);
        }

        private void ApplyInternal(Player player, bool isLocal)
        {
            if (!isLocal && !NetworkUtils.IsServer())
            {
                return;
            }

            if (player.GetComponent<PlayerModel>())
            {
                UnityEngine.Object.Destroy(player.GetComponent<PlayerModel>());
            }

            var oldModel = player.Model.gameObject;
            var newModel = LoadModel(player);

            newModel.Model.gameObject.name = player.FormatDebugName();
            var transform = newModel.Model.transform;

            transform.rotation = oldModel.transform.rotation;
            transform.position = oldModel.transform.position;

            player.SendMessage("OnPlayerModelChange", newModel, SendMessageOptions.DontRequireReceiver);
            player.OnPlayerModelChange(newModel);

            var container = player.gameObject.transform;
            container.SetParent(transform);

            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            UnityEngine.Object.Destroy(oldModel);

            if (NetworkUtils.IsServer())
            {
                PlayerModelControllerNetwork.Instance.OnModelChange(player, this);
            }
        }

        protected abstract PlayerModel LoadModel(Player player);
    }
}