using Static_Interface.API.NetworkFramework;
using Static_Interface.Internal;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PlayerBehaviour : NetworkedBehaviour
    {
         public Player Player { get; protected set; }

         protected override void Awake()
         {
            base.Awake();
            Player = GetComponent<Player>();
         }
    }
}