namespace Static_Interface.Multiplayer.Client
{
    public abstract  class Client
    {
        public abstract void Connect(uint ip, ushort port, string password);
    }
}