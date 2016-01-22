namespace Static_Interface.API.EventFramework
{
    public interface ICancellable
    {
        bool IsCancelled { get; set; }
    }
}