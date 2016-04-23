using System;

namespace Static_Interface.API.Utils
{
    public class TimeUtil
    {
        public static uint GetCurrentTime()
        {
            return (uint) DateTime.UtcNow.Millisecond;
        } 
    }
}