using System.Collections.Generic;
using System.Collections.ObjectModel;
using Static_Interface.API.InteractionFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SerializationFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Server;
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

        public int CurrentSeat => Passengers.Count-1;

        public Player Driver
        {
            get { return _driver; }
            protected set
            {
                _driver = value;
                if (value == null)
                {
                    Channel.Owner = Connection.ServerID;
                    if (IsServer()) Channel.Send(nameof(Network_SetDriver), ECall.Others, Connection.ServerID);
                    return;
                }

                Channel.Owner = _driver.User.Identity;
                if(IsServer()) Channel.Send(nameof(Network_SetDriver), ECall.Others, _driver.User.Identity);
            }
        }

        public string ID { get; internal set; }

        //Todo: implement these
        public bool IsDestroyed => Health == 0;
        public bool Destroyable { get; set; }
        public int MaxHealth { get; set; } = 100;
        public int Health { get; set; } = 100;
        protected override void Awake()
        {
            base.Awake();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody)
                Rigidbody = gameObject.AddComponent<Rigidbody>();
            var syncer = gameObject.GetComponent<RigidbodyPositionSyncer>();
            if (!syncer)
                gameObject.AddComponent<RigidbodyPositionSyncer>();
        }

        protected override void OnInteract(Player player)
        {
            if (player != Player.MainPlayer && !IsServer()) return;
            AddPassenger(player);
        }

        [NetworkCall(ValidateServer = true)]
        private void Network_SetDriver(Identity ident, Identity driver)
        {
            Driver = driver?.Owner?.Player;
        }

        [NetworkCall]
        private void Network_AddPassenger(Identity ident, Identity passenger)
        {
            if (ident != Connection.ServerID)
            {
                if (!IsServer()) return;
                if (ident != passenger) return;
            }

            AddPassenger(passenger.Owner.Player, false);
        }

        [NetworkCall]
        private void Network_ExitPassenger(Identity ident, Identity passenger)
        {
            if (ident != Connection.ServerID)
            {
                if (!IsServer()) return;
                if (ident != passenger) return;
            }

            ExitPassenger(passenger.Owner.Player, false);
        }

        protected override void Update()
        {
            base.Update();
            if (IsPassenger(Player.MainPlayer) && Input.GetKeyDown(InteractManager.Instance.InteractKey))
            {
                ExitPassenger(Player.MainPlayer);
            }
        }

        public bool IsPassenger(Player player)
        {
            return _passengers.Contains(player);
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
            StartEngine(false);
        }

        private void StartEngine(bool isCommand)
        {
            if(IsEngineStarted) return;
            //todo: on engine start event
            if (OnEngineStart())
            {
                IsEngineStarted = true;
            }

            if (IsServer())
            {
                Channel.Send(nameof(Network_StartEngine), ECall.NotOwner);
            }
            else if (!isCommand)
            {
                Channel.Send(nameof(Network_StartEngine), ECall.Server);
            }
        }

        public void StopEngine()
        {
            StopEngine(false);
        }

        private void StopEngine(bool isCommand)
        {
            if (!IsEngineStarted) return;

            //todo: on engine stop event
            if (OnEngineStop())
            {
                IsEngineStarted = false;
            }

            if (IsServer())
            {
                Channel.Send(nameof(Network_StopEngine), ECall.NotOwner);
            }
            else if (!isCommand)
            {
                Channel.Send(nameof(Network_StopEngine), ECall.Server);
            }
        }

        [NetworkCall]
        private void Network_StartEngine(Identity ident)
        {
            if (!Channel.ValidateServer(ident, false) && !Channel.ValidateOwner(ident, false))
                return;
            StartEngine(true);
        }

        [NetworkCall]
        private void Network_StopEngine(Identity ident)
        {
            if (!Channel.ValidateServer(ident, false) && !Channel.ValidateOwner(ident, false))
                return;
            StopEngine(true);
        }

        protected abstract bool OnEngineStart();
        protected abstract bool OnEngineStop();

        public bool ExitPassenger(Player player)
        {
            return ExitPassenger(player, true);
        }

        private bool ExitPassenger(Player player, bool sendNetworkRequest)
        {
            if (!_passengers.Contains(player)) return false;
            //todo: on passender exit event
            if (!OnExitPassenger(player)) return false;
            RestoreCamera(player);
            if (Driver == player) Driver = null;
            player.transform.parent = null;
            player.Model.GetComponent<Rigidbody>().isKinematic = false;
            player.Model.GetComponent<Rigidbody>().useGravity = true;
            player.Model.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            player.Model.GetComponent<Rigidbody>().detectCollisions = true;
            player.Model.GetComponent<RigidbodyPositionSyncer>().enabled = true;
            player.Vehicle = null;

            if (IsServer())
            {
                Channel.Send(nameof(Network_ExitPassenger), ECall.Others, player.User.Identity);
            }
            else if (sendNetworkRequest && player == Player.MainPlayer)
            {
                Channel.Send(nameof(Network_ExitPassenger), ECall.Server, player.User.Identity);
            }
            return true;
        }

        protected abstract bool OnExitPassenger(Player player);

        public bool AddPassenger(Player player)
        {
            return AddPassenger(player, true);
        }

        private bool AddPassenger(Player player, bool sendNetworkRequest)
        {
            if (IsDestroyed || _passengers.Contains(player) || IsFull) return false;
            //todo: on passenger add event
            bool wasEmpty = IsEmpty;

            player.transform.parent = transform;
            if (!OnAddPassenger(player))
            {
                player.transform.parent = null;
                return false;
            }

            _passengers.Add(player);
            player.GetComponent<PlayerInputController>().DisableControl();
            player.Model.GetComponent<Rigidbody>().isKinematic = true;
            player.Model.GetComponent<Rigidbody>().useGravity = false;
            player.Model.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            player.Model.GetComponent<Rigidbody>().detectCollisions = false;
            player.Model.GetComponent<RigidbodyPositionSyncer>().enabled = false;
            player.Vehicle = this;
            if (wasEmpty && !IsEngineStarted)
            {
                Driver = player;
                StartEngine();
            }
            _camerasBefore.Add(player, player.Camera);
            SetCamera(player);

            if (IsServer())
            {
                Channel.Send(nameof(Network_AddPassenger), ECall.Others, player.User.Identity);
            }
            else if (sendNetworkRequest && player == Player.MainPlayer)
            {
                Channel.Send(nameof(Network_ExitPassenger), ECall.Server, player.User.Identity);
            }
            return true;
        }

        protected abstract bool OnAddPassenger(Player player);

        public override string GetInteractMessage()
        {
            return "Enter " + Name;
        }

        public override bool CanInteract(Player player)
        {
            return !IsDestroyed && !_passengers.Contains(player) && !IsFull;
        }

        public bool IsFull => _passengers.Count >= MaxPassengers;
        public bool IsEmpty => _passengers.Count == 0;

        public abstract int MaxPassengers { get; }

        private readonly List<Player> _passengers = new List<Player>();
        public ReadOnlyCollection<Player> Passengers => _passengers.AsReadOnly();

        public override Mesh InteractableObject => ObjectUtils.GetCombinedMesh(gameObject);
    }
}