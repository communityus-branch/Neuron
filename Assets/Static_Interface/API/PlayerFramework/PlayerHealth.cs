using System;
using Static_Interface.API.GUIFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeaponFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerHealth : PlayerBehaviour
    {
        public const float MIN_COLLISION_MOMENTUM = 50;
        private Rigidbody _rigidbody;
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
            var deathCause = EPlayerDeathCause.COLLISION;
            var hitTransform = collision.transform;
            if (hitTransform.GetComponent<Bullet>() != null)
            {
                var bullet = hitTransform.GetComponent<Bullet>();
                Physics.IgnoreCollision(GetComponent<Collider>(), hitTransform.GetComponent<Collider>());
                if (bullet.Damage != null)
                {
                    bool wasDead = IsDead;
                    Player.Health.DamagePlayer((int) bullet.Damage.Value, null);
                    if (!wasDead && IsDead)
                    {
                        OnPlayerDeath(EPlayerDeathCause.SHOT);
                    }
                    return;
                }
                //If bullet.Damage == null we will calculate damamge from collision 
                deathCause = EPlayerDeathCause.SHOT;
            }

            var momentum = collision.relativeVelocity * _rigidbody.mass;
            if (momentum.magnitude > MIN_COLLISION_MOMENTUM * _rigidbody.mass)
            {
                Player.Health.PlayerCollision(momentum, deathCause);
            }
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            _rigidbody = GetComponent<Rigidbody>();
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
                    _rigidbody.freezeRotation = false;
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

                if (newHealth < _health)
                {
                    DamagePlayer(_health - newHealth, null, true);
                }
                else
                {
                    _health = newHealth;
                }
                if (_healthProgressBarView != null)
                    _healthProgressBarView.Value = newHealth;

                if (IsServer() && !Channel.IsOwner)
                {
                    Channel.Send(nameof(Network_SetHealth), target, MaxHealth, Health);
                }
                if (!isStatusUpdate) return;
            }
        }

        public void Kill(EPlayerDeathCause deathcause)
        {
            if (IsServer())
            {
                Channel.Send(nameof(Network_Kill), ECall.Clients);
            }
            Kill(deathcause, false);
        }

        private void Kill(EPlayerDeathCause deathcause, bool ignoreServer)
        {
            if (!ignoreServer && !IsServer()) return;
            Health = 0;
            OnPlayerDeath(deathcause);
        }

        private void OnPlayerDeath(EPlayerDeathCause reason, object arg = null)
        {
            if (IsServer())
            {
                Chat.Instance.SendServerMessage("<b>" + Player.User.Name + "</b> died (" + reason + ")");
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

            _rigidbody.rotation = Quaternion.identity;
            _rigidbody.freezeRotation = true;
            Player.MovementController?.EnableControl();

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

        public void PlayerCollision(Vector3 momentum, EPlayerDeathCause deathCause = EPlayerDeathCause.COLLISION)
        {
            if (!IsServer()) return;
            bool wasDead = IsDead;
            var magnitude = momentum.magnitude;
            var speed = magnitude / GetComponent<Rigidbody>().mass;
            var damage = (10 / 4) * speed;  // 100 damage at 40 m/s
            DamagePlayer((int)damage, null);
            if (!wasDead && IsDead)
            {
                OnPlayerDeath(deathCause);
            }
        }

        public void DamagePlayer(int damage, EPlayerDeathCause? cause = EPlayerDeathCause.UNKNOWN)
        {
            DamagePlayer(damage, cause, false);
        }

        protected void DamagePlayer(int damage, EPlayerDeathCause? cause, bool internallCall)
        {
            if (!internallCall && !IsServer()) return;
            bool wasDead = IsDead;
            if (internallCall) _health -= damage;
            else Health -= damage;
            if (cause != null && IsDead && !wasDead)
            {
                OnPlayerDeath(cause.Value);
            }
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SetHealth(Identity identity, int maxhealth, int health)
        {
            MaxHealth = maxhealth;
            Health = health;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_Kill(Identity identity, int cause)
        {
            if (identity == Connection.ClientID) return;
            Kill((EPlayerDeathCause) cause, true);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_Revive(Identity identity, int health)
        {
            if (identity == Connection.ClientID) return;
            RevivePlayer(health, true);
        }
    }
}