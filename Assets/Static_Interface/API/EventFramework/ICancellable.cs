namespace Static_Interface.API.EventFramework
{
    /// <summary>
    /// Makes an <see cref="Event"/> cancellable if implemented
    /// </summary>
    public interface ICancellable
    {
        /// <summary>
        /// Should the event be cancelled? Also if this is set to true, listener methods which doesn't have <see cref="EventHandler.IgnoreCancelled"/> 
        /// set to true won't receive the event.
        /// </summary>
        bool IsCancelled { get; set; }
    }
}