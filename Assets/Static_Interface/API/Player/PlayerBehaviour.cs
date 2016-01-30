using Static_Interface.API.Network;

namespace Static_Interface.API.Player
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