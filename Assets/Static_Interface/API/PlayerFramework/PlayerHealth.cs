using System;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.EventFramework;
using Static_Interface.API.GUIFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework.Events;
using Static_Interface.API.Utils;
using Static_Interface.API.WeaponFramework;
using Static_Interface.Internal;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerHealth : PlayerBehaviour
    {
        public const float MIN_COLLISION_MOMENTUM = 50;
        private Rigidbody Rigidbody => Player.Model.GetComponent<Rigidbody>();
        private ProgressBarView _healthProgressBarView;
        protected override void Awake()
        {
            base.Awake();
            MaxHealth = 100;
            _health = 100;
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            var momentum = collision.relativeVelocity * Rigidbody.mass;
            var deathCause = EDamageCause.COLLISION;
            var hitTransform = collision.transform;
            bool isBullet = hitTransform.GetComponent<Projectile>() != null;

            PlayerCollisionEvent @event = new PlayerCollisionEvent(Player, collision, momentum);
            @event.IsBullet = isBullet;
            EventManager.Instance.CallEvent(@event);
            if (@event.IsCancelled) return;

            if (isBullet)
            {
                var bullet = hitTransform.GetComponent<Projectile>();
                Physics.IgnoreCollision(GetComponent<Collider>(), hitTransform.GetComponent<Collider>());
                if (bullet.Damage != null)
                {
                    Player.Health.DamagePlayer(bullet.Damage.Value, EDamageCause.SHOT, bullet.Owner);
                    return;
                }
                //If bullet.Damage == null we will calculate damamge from collision 
                deathCause = EDamageCause.SHOT;
            }

            if (momentum.magnitude > MIN_COLLISION_MOMENTUM * Rigidbody.mass)
            {
                Player.Health.PlayerCollision(momentum, deathCause);
            }
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (!UseGUI()) return;
            _healthProgressBarView = new ProgressBarView("Health", Player.GUI.RootView)
            {
                MaxValue = MaxHealth,
                MinValue = 0,
                Value = Health,
                Label = "Health"
            };
            Player.GUI.AddStatusProgressBar(_healthProgressBarView);
        }

        public bool IsDead => Health == 0;

        private int _health;
        public int Health
        {
            get { return _health; }
            private set
            {
                bool wasDead = _health == 0;
                int newHealth = Mathf.Clamp(value, 0, MaxHealth);

                if (!wasDead && newHealth == 0)
                {
                    Rigidbody.freezeRotation = false;
                    Player.MovementController?.DisableControl();
                }

                if (IsDead && newHealth > 0)
                {
                    RevivePlayer(newHealth);
                }

                ECall target = ECall.Owner;
                bool isStatusUpdate = false;
                if ((_health > 0 && newHealth == 0) || (_health == 0 && newHealth > 0))
                {
                    target = ECall.Clients;
                    isStatusUpdate = true;
                }

                _health = newHealth;
                if (_healthProgressBarView != null)
                    _healthProgressBarView.Value = newHealth;

                if (!isStatusUpdate) return;
                if (IsServer() && !Channel.IsOwner)
                {
                    Channel.Send(nameof(Network_SetHealth), target, MaxHealth, Health);
                }
            }
        }

        public void Kill(EDamageCause deathcause)
        {
            if (IsServer())
            {
                Channel.Send(nameof(Network_Kill), ECall.Clients, (int)deathcause);
            }
            Kill(deathcause, false);
        }

        private void Kill(EDamageCause deathcause, bool ignoreServer)
        {
            if (!ignoreServer && !IsServer()) return;
            Health = 0;
            OnPlayerDeath(deathcause, null);
        }

        private void OnPlayerDeath(EDamageCause reason, IEntity killedBy)
        {
            PlayerDeathEvent @event = new PlayerDeathEvent(Player);
            @event.DeathMessage = "<b>" + Player.User.Name + "</b> died (" + reason + ")";
            @event.Killer = killedBy;
            @event.DeathCause = reason;
            EventManager.Instance.CallEvent(@event);
            if (@event.DeathMessage != null && IsServer())
            {
                Chat.Instance.SendServerMessage(@event.DeathMessage);
            }
        }

        public void RevivePlayer()
        {
            RevivePlayer(MaxHealth);
        }


        public void RevivePlayer(int health)
        {
            RevivePlayer(health, false);   
        }

        private void RevivePlayer(int health, bool ignoreServer)
        {
            if (!ignoreServer && !IsServer()) return;

            if (health <= 0)
            {
                throw new ArgumentException("value can not be <= 0", nameof(health));
            }
            if (!IsDead)
            {
                LogUtils.LogWarning("RevivePlayer: Player is not dead!");
                return;
            }

            _health = health;

            Rigidbody.rotation = Quaternion.identity;
            Rigidbody.freezeRotation = true;
            Player.MovementController?.EnableControl();

            PlayerReviveEvent @event = new PlayerReviveEvent(Player);
            EventManager.Instance.CallEvent(@event);

            if (IsServer() && !Channel.IsOwner)
            {
                Channel.Send(nameof(Network_Revive), ECall.Clients, _health);
            }
        }
        
        private int _maxHealth;

        public int MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                if (_healthProgressBarView != null)
                    _healthProgressBarView.MaxValue = MaxHealth;
            }
        }

        public void PlayerCollision(Vector3 momentum, EDamageCause deathCause = EDamageCause.COLLISION)
        {
            if (!IsServer()) return;
            var magnitude = momentum.magnitude;
            var speed = magnitude / GetComponent<Rigidbody>().mass;
            var damage = (10 / 4) * speed;  // 100 damage at 40 m/s
            DamagePlayer((int)damage, deathCause, World.Instance);
        }


        public void DamagePlayer(int damage, EDamageCause cause, IEntity damagingEntity = null)
        {         
            PlayerDamageEvent @event = new PlayerDamageEvent(Player);
            @event.Damage = damage;
            @event.DamageCausingEntity = damagingEntity;
            @event.DamageCause = cause;
            EventManager.Instance.CallEvent(@event);

            bool wasDead = IsDead;
            Health -= @event.Damage;

            if (IsDead && !wasDead)
            {
                OnPlayerDeath(@event.DamageCause, damagingEntity);
            }
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetHealth(Identity identity, int maxhealth, int health)
        {
            MaxHealth = maxhealth;
            if (Health > health)
            {
                //Todo: Networked damaging!!
                DamagePlayer(Health - health, EDamageCause.UNKNOWN);
            }
            else
            {
                Health = health;
            }
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_Kill(Identity identity, int cause)
        {
            if (identity == Connection.ClientID) return;
            Kill((EDamageCause) cause, true);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_Revive(Identity identity, int health)
        {
            if (identity == Connection.ClientID) return;
            RevivePlayer(health, true);
        }
    }
}