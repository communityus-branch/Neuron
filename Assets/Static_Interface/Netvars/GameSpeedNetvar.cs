using Static_Interface.API.Netvar;

namespace Static_Interface.Netvars
{
    public class GameSpeedNetvar : Netvar
    {
        private readonly float _defaultValue;
        public GameSpeedNetvar()
        {
            _defaultValue = 1;
        }
        public override string Name
        {
            get { return "sv_speed"; }
        }

        public override object GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}