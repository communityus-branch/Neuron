using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.WeaponFramework
{
    public class DummyWeapon : Weapon
    {
        private AudioSource _audioSource;
        private readonly AudioClip _clip = Resources.Load<AudioClip>("Sounds/SniperRiffleSound");
        int _bulletCount = 1;
        public override void Use()
        {
            GameObject bullet = (GameObject) Object.Instantiate(Resources.Load("SimpleBullet"));
            bullet.transform.rotation = Quaternion.Euler(Player.transform.TransformPoint(new Vector3(-90, 0, 0)));
            bullet.name = "Bullet #" + _bulletCount;
            var bullcomp = bullet.AddComponent<Projectile>();
            bullcomp.Owner = Player;
            bullet.transform.rotation = Player.transform.rotation;
            bullet.transform.position = Player.transform.TransformPoint(Vector3.forward/1000);

            var rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(bullet.transform.TransformDirection(Vector3.forward)*10000 * bullet.GetComponent<Rigidbody>().mass);
            var destroy = bullet.AddComponent<TimedDestroy>();
            destroy.DestroyAfter(10 * 1000);
            _bulletCount++;


            _audioSource.PlayOneShot(_clip);
        }

        public override void OnEquip()
        {
            _audioSource = Controller.AudioSource;
            _audioSource.volume = 0.5f;
        }

        public DummyWeapon(WeaponController controller, Player player) : base(controller, player)
        {
        }
    }
}