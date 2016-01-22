using Static_Interface.API.NetvarFramework;

namespace Static_Interface.The_Collapse.Netvars
{
    public class GameSpeedNetvar : Netvar
    {
        public override string Name => "sv_speed";

        public override object GetDefaultValue()
        {
            return 1;
        }
    }
}