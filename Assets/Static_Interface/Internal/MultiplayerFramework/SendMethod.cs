namespace Static_Interface.Internal.MultiplayerFramework
{
    public enum SendMethod : int
    {
        SEND_UNRELIABLE = 0,
        SEND_UNRELIABLE_NO_DELAY = 1,
        SEND_RELIABLE = 2,
        SEND_RELIABLE_WITH_BUFFERING = 3,
    }
}