namespace Assets.API.EventFramework
{
    public interface ICancellable
    {
        bool IsCancelled();
        void SetCancelled(bool cancel);
    }
}