using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.WeaponFramework
{
    public abstract class Weapon
    {
        public readonly Player Player;
        public Texture2D Crosshair { get; protected set; } = null;
        public uint? CrosshairScale { get; protected set; } = null;
        public bool DrawCrosshair { get; protected set; } = true;
        protected Weapon(Player player)
        {
            Player = player;
        }

        public bool SingleShot { get; protected set; }
        public uint FireCooldown { get; protected set; } = 750;
        public abstract void Use();
        public virtual bool CanUse()
        {
            return true;
        }

        public virtual void OnUnequip()
        {

        }

        public virtual void OnEquip()
        {
            
        }

        public virtual void FixedUpdate()
        {

        }

        public virtual void Update()
        {
            
        }

        public virtual void OnGUI()
        {
            
        }
    }
}