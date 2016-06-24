using System.Linq;
using Static_Interface.API.CommandFramework;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.VehicleFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class Player : UnityExtensions.MonoBehaviour, IEntity, ICommandSender
    {
        public Transform Model => PlayerModel == null ? transform.parent : PlayerModel.Model.transform;
        public static Player MainPlayer { get; internal set; } = null;
        public PlayerController MovementController => GetComponent<PlayerController>();
        public PlayerHealth Health => GetComponent<PlayerHealth>();
        public User User { get; internal set; }

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
    }
}