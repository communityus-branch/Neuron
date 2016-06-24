using System.Linq;
using Static_Interface.API.CommandFramework;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.EventFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework.Events;
using Static_Interface.API.VehicleFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class Player : UnityExtensions.MonoBehaviour, IEntity, ICommandSender, IListener
    {
        protected override void Awake()
        {
            base.Awake();
            EventManager.Instance.RegisterEventsInternal(this, null);
            gameObject.AddComponent<RigidbodyPositionSyncer>();
            CheckRigidbody();
        }

        public Transform Model => PlayerModel == null ? transform.parent : PlayerModel.Model.transform;
        public static Player MainPlayer { get; internal set; } = null;
        public PlayerInputController MovementController => GetComponent<PlayerInputController>();
        public PlayerHealth Health => GetComponent<PlayerHealth>();
        public User User { get; internal set; }
        public Rigidbody Rigidbody => Model?.GetComponent<Rigidbody>();
        public Camera Camera => GetComponentsInChildren<Camera>().FirstOrDefault(c => c.enabled);
        public Channel Channel => GetComponent<Channel>();
        public PlayerGUI GUI => GetComponent<PlayerGUI>();
        public PlayerModel PlayerModel => transform.parent?.GetComponent<PlayerModel>();
        public string Name => User.Name;
        public Vehicle Vehicle { get; internal set; }

        public bool HasPermission(string permission)
        {
            return true; //Todo
        }

        public void Message(string msg)
        {
            Chat.Instance.SendMessageToPlayer(this, msg);
        }

        public string CommandPrefix { get; set; } = "/";

        public string FormatDebugName()
        {
            return FormatDebugName(User.Name, Channel.ID);
        }

        public static string FormatDebugName(string playerName, int channel)
        {
            return playerName + " @ ch-" + channel;
        }

        //send our current model to the player who connected
        [EventHandler(Priority = EventPriority.LOWEST)]
        public void OnPlayerJoin(PlayerJoinEvent @event)
        {
            if (!NetworkUtils.IsServer()) return;
            var player = @event.Player;
            if (this == player) return;
            if (PlayerModel == null) return;
            PlayerModelControllerNetwork.Instance.SendUpdate(player.User.Identity, this, PlayerModel.PlayerModelController);
        }

        internal void OnPlayerModelChange(PlayerModel newModel)
        {
            CheckRigidbody(newModel.Model);
            GetComponent<RigidbodyPositionSyncer>().RigidbodyToSync = newModel.Model.GetComponent<Rigidbody>();
            GetComponent<AngleSyncer>().TransformToSync = newModel.Model.transform;
        }

        public void CheckRigidbody()
        {
            CheckRigidbody(Model.gameObject);
        }

        private void CheckRigidbody(GameObject model)
        {
            var rigidbody = model.GetComponent<Rigidbody>();
            if (!rigidbody)
            {
                rigidbody = model.AddComponent<Rigidbody>();
                rigidbody.mass = 80;
                rigidbody.freezeRotation = true;
                GetComponent<RigidbodyPositionSyncer>().RigidbodyToSync = rigidbody;
                GetComponent<AngleSyncer>().TransformToSync = rigidbody.transform;
            }
        }
    }
}