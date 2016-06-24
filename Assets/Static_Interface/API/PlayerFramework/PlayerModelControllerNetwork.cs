using Static_Interface.API.NetworkFramework;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerModelControllerNetwork : NetworkedSingletonBehaviour<PlayerModelControllerNetwork>
    {
        public void OnModelChange(Player player, PlayerModelController controller)
        {
            if (!NetworkUtils.IsServer()) return;
            Channel.Send(nameof(Network_ChangeModel), ECall.Others, player.User.Identity, controller.Name);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_ChangeModel(Identity sender, Identity target, string modelController)
        {
            var player = target.Owner.Player;
            var controller = PlayerModelController.GetPlayerModelController(modelController);
            controller.ApplyLocal(player);
        }

        public void SendUpdate(Identity target, Player toUpdate, PlayerModelController updateModel)
        {
            Channel.Send(nameof(Network_ChangeModel), target, toUpdate.User.Identity, updateModel.Name);
        }
    }
}