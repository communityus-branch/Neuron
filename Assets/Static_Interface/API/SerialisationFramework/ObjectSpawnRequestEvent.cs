using Static_Interface.API.EventFramework;
using UnityEngine;
using Event = Static_Interface.API.EventFramework.Event;

namespace Static_Interface.API.SerialisationFramework
{
    public class ObjectSpawnRequestEvent : Event, ICancellable
    {
        public string Bundle { get; set; }
        public string Asset { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public uint CallbackID { get; set; }

        public ObjectSpawnRequestEvent(string bundle, string asset, Vector3 position, Quaternion rotation, uint callbackId)
        {
            Bundle = bundle;
            Asset = asset;
            Position = position;
            Rotation = rotation;
            CallbackID = callbackId;
        }

        public bool IsCancelled { get; set; }
    }
}