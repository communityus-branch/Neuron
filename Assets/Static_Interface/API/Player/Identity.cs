namespace Static_Interface.API.Player
{
    public abstract class Identity
    {
        public User Owner { get; internal set; }
        public abstract bool IsValid();
        public abstract ulong Serialize();
    }
}