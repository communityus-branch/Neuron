using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.WeaponFramework
{
    public class DummyWeapon : Weapon
    {
        private readonly AudioClip _clip = Resources.Load<AudioClip>("Sounds/SniperRiffleSound");
        int _bulletCount = 1;
        public override void Use()
        {
            GameObject bullet = (GameObject) Object.Instantiate(Resources.Load("SimpleBullet"));
            bullet.name = "Bullet #" + _bulletCount;
            var bullcomp = bullet.AddComponent<Bullet>();
            bullcomp.Owner = Player;
            bullet.transform.rotation = Player.transform.rotation;
            bullet.transform.position = Player.transform.TransformPoint(Vector3.forward/1000);

            var rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(bullet.transform.TransformDirection(Vector3.forward)*10000);
            var destroy = bullet.AddComponent<TimedDestroy>();
            destroy.DestroyAfter(10 * 1000);
            _bulletCount++;

            var audioSource = Player.GetComponent<AudioSource>();

            audioSource.PlayOneShot(_clip);
        }

        public DummyWeapon(Player player) : base(player)
        {
        }
    }
}