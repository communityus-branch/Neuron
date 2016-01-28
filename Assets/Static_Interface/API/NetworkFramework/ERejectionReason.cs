namespace Static_Interface.API.NetworkFramework
{
    public enum ERejectionReason
    {
        SERVER_FULL,
        WRONG_VERSION,
        ALREADY_PENDING,
        ALREADY_CONNECTED,
        NOT_PENDING,
        LATE_PENDING, //todo
        AUTH_VERIFICATION,
        AUTH_VAC_BAN, 
        AUTH_TIMED_OUT,
        AUTH_PUB_BAN,
        AUTH_NO_STEAM,
        AUTH_LICENSE_EXPIRED,
        AUTH_ELSEWHERE,
        AUTH_USED,
        AUTH_NO_USER,
        PING, //todo
        TIMEOUT
    }
}