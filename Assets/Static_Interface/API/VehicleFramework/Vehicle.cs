using System.Collections.Generic;
using System.Collections.ObjectModel;
using Static_Interface.API.InteractionFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SerializationFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.VehicleFramework
{
    public abstract class Vehicle : Interactable
    {
        //todo sync position
        protected abstract Camera Camera { get; }
        private readonly Dictionary<Player, Camera> _camerasBefore = new Dictionary<Player, Camera>();
        public Rigidbody Rigidbody { get; private set; }
        public bool IsEngineStarted { get; private set; }

        private Player _driver;

        public Player Driver
        {
            get { return _driver; }
            protected set
            {
                _driver = value;
                if (value == null)
                {
                    Channel.Owner = Connection.ServerID;
                    return;
                }

                Channel.Owner = _driver.User.Identity;
            }
        }

        public string ID { get; internal set; }

        //Todo: implement these
        public bool IsDestroyed => Health == 0;
        public bool Destroyable { get; set; }
        public int Health { get; set; } //Todo: OnHealthSet

        protected override void Awake()
        {
            base.Awake();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody)
                Rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        protected override void OnInteract(Player player)
        {
            bool wasEmpty = IsEmpty;

            var rotBefore = player.transform.rotation;
            player.transform.parent = gameObject.transform;
            player.transform.localRotation = Quaternion.identity;
            if (AddPassenger(player))
            {
                player.GetComponent<PlayerController>().DisableControl();
                _passengers.Add(player);
                if (wasEmpty && !IsEngineStarted)
                {
                    Driver = player;
                    StartEngine();
                }
                _camerasBefore.Add(player, player.Camera);
                SetCamera(player);
            }
            else
            {
                player.transform.parent = null;
                player.transform.rotation = rotBefore;
            }
        }

        protected virtual void SetCamera(Player player)
        {
            if (player == Player.MainPlayer)
            {
                CameraManager.Instance.CurrentCamera = Camera;
            }
        }

        private void RestoreCamera(Player player)
        {
            if (player == Player.MainPlayer)
            {
                CameraManager.Instance.CurrentCamera = _camerasBefore[player];
            }
            _camerasBefore.Remove(player);
        }


        public void StartEngine()
        {
            if(IsEngineStarted) return;
            //todo: on engine start event
            if (OnEngineStart())
            {
                IsEngineStarted = true;
            }
        }
        
        public void StopEngine()
        {
            if (!IsEngineStarted) return;
            //todo: on engine stop event
            if (OnEngineStop())
            {
                IsEngineStarted = false;
            }
        }

        protected abstract bool OnEngineStart();
        protected abstract bool OnEngineStop();

        public bool ExitPassenger(Player player)
        {
            if (!_passengers.Contains(player)) return false;
            //todo: on passender exit event
            if (!OnExitPassenger(player)) return false;
            RestoreCamera(player);
            if (Driver == player) Driver = null;
            player.transform.parent = null;
            return true;
        }

        protected abstract bool OnExitPassenger(Player player);

        protected override void UpdateClient()
        {
            base.UpdateClient();

            if (!Passengers.Contains(Player.MainPlayer)) return;
            //todo: handle input
        }

        public bool AddPassenger(Player player)
        {
            if (IsDestroyed || _passengers.Contains(player) || IsFull) return false;
            //todo: on passenger add event
            return OnAddPassenger(player);
        }

        protected abstract bool OnAddPassenger(Player player);

        public override string GetInteractMessage()
        {
            return "Enter " + Name;
        }

        public override bool CanInteract(Player player)
        {
            return !_passengers.Contains(player) && !IsFull;
        }

        public bool IsFull => _passengers.Count >= MaxPassengers;
        public bool IsEmpty => _passengers.Count == 0;

        public abstract int MaxPassengers { get; }

        private readonly List<Player> _passengers = new List<Player>();
        public ReadOnlyCollection<Player> Passengers => _passengers.AsReadOnly();

        public override Mesh InteractableObject => ObjectUtils.GetCombinedMesh(gameObject);
    }
}