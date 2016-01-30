namespace Static_Interface.API.Event
{
    public interface ICancellable
    {
        bool IsCancelled { get; set; }
    }
}