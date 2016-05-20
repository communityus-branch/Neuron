using Static_Interface.API.EntityFramework;
using Static_Interface.API.UnityExtensions;

namespace Static_Interface.API.WeaponFramework
{
    public class Projectile : MonoBehaviour
    {
        public int? Damage = null;
        public IEntity Owner { get; set; }
    }
}