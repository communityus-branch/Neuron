using Static_Interface.Multiplayer.Protocol;

namespace Static_Interface.PlayerFramework
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