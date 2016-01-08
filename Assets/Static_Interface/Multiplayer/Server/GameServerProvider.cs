namespace Static_Interface.Multiplayer.Server
{
    public abstract class GameServerProvider
    {
        public string Description = "A " + Game.NAME + " Server";
        private bool hosted;

        public void Start(uint ip, ushort port, bool dedicated)
        {
            if (hosted) return;
            OnStart(ip, port, dedicated);
            hosted = true;
        }

        protected abstract void OnStart(uint ip, ushort port, bool dedicated);

        public void Stop()
        {
            if (!hosted) return;
            OnStop();
            hosted = false;
        }

        protected abstract void OnStop();
    }
}
