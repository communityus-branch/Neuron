using System;

namespace Static_Interface.API.Utils
{
    public class Random64
    {
        private readonly Random _random;

        public Random64()
        {
            _random = new Random();
        }

        public ulong Next()
        {
            return Next(UInt64.MaxValue);
        }

        public ulong Next(ulong maxValue)
        {
            return Next(0, maxValue);
        }

        public ulong Next(ulong minValue, ulong maxValue)
        {
            if (maxValue < minValue)
                throw new ArgumentException("minValue > maxValue");
            if (minValue < 0)
                throw new ArgumentException("minValue < 0");
            return (ulong)(_random.NextDouble() * (maxValue - minValue)) + minValue;
        }
    }

}