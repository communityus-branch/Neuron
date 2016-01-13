namespace Static_Interface.Multiplayer.Service
{
    public abstract class Service : IService
    {
        public virtual void Initialize() {}
        public virtual void Shutdown() { }
        public virtual void Update() { }
    }
}