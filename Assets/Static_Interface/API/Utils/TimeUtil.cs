using System;

namespace Static_Interface.API.Utils
{
    public class TimeUtil
    {
        public static long GetCurrentTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        } 
    }
}