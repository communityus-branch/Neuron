using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.WeaponFramework
{
    public class WeaponController : PlayerBehaviour
    {
        private long _lastUsage;
        public bool DrawCrosshair = true;
        protected override bool IsSyncable => true;
        public AudioSource AudioSource { get; protected set; }
        protected override void Awake()
        {
            base.Awake();
            SetCurrentWeapon(new DummyWeapon(this, Player));
        }

        public Weapon CurrentWeapon { get; private set; }
        protected override bool OnSync()
        {
            base.OnSync();
            if (Player.Health.IsDead) return false;
            if (InputUtil.Instance.IsInputLocked(this)) return false;
            if (CurrentWeapon == null) return false;

            if ((CurrentWeapon.SingleShot && Input.GetKeyDown(KeyCode.Mouse0)) || (!CurrentWeapon.SingleShot && Input.GetKey(KeyCode.Mouse0)))
            {
                UseWeapon();
            }

            return true;
        }

        public void UseWeapon()
        {
            if (!CanUseCurrentWeapon()) return;
            Channel.Send(nameof(Network_UseServer), ECall.Server);
            _lastUsage = TimeUtil.GetCurrentTime();
            if(!Internal.MultiplayerFramework.Connection.IsSinglePlayer) CurrentWeapon.Use();
        }

        public bool CanUseCurrentWeapon()
        {
            if (CurrentWeapon == null) return false;
            return TimeUtil.GetCurrentTime() - _lastUsage >= CurrentWeapon.FireCooldown && CurrentWeapon.CanUse();
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        private void Network_UseServer(Identity ident)
        {
            if (!CanUseCurrentWeapon()) return;
            CurrentWeapon.Use();
            Channel.Send(nameof(Network_UseClient), ECall.NotOwner);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true, MaxRadius = 500f)]
        private void Network_UseClient(Identity ident)
        {
            CurrentWeapon.Use();
        }

        internal void SetCurrentWeapon(Weapon weapon)
        {
            //Reset audio
            if(AudioSource != null && AudioSource) Destroy(AudioSource);
            AudioSource = gameObject.AddComponent<AudioSource>();
            CurrentWeapon?.OnUnequip();
            CurrentWeapon = weapon;
            weapon?.OnEquip();
        }

        protected override void Update()
        {
            base.Update();
            CurrentWeapon?.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            CurrentWeapon?.FixedUpdate();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            CurrentWeapon?.OnGUI();
            if (!IsLocalPlayer || CurrentWeapon == null) return;
            if (!DrawCrosshair) return;
            if (!CurrentWeapon.DrawCrosshair) return;

            var texture = CurrentWeapon.Crosshair;;
            if (texture == null)
            {
                GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
                return;
            }

            var scale = CurrentWeapon.CrosshairScale ?? 1;
            GUI.DrawTexture(new Rect((
                Screen.width - texture.width * scale) / 2, 
                (Screen.height - texture.height * scale) / 2,
                texture.width * scale,
                texture.height * scale), texture);
        }
    }
}