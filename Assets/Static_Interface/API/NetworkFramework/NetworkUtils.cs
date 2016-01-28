namespace Static_Interface.API.NetworkFramework
{
    public static class NetworkUtils
    {
        public static void GetAveragePing(float currentPing, out float ping, float[] cache)
        {
            ping = currentPing;
            for (var i = cache.Length - 1; i > 0; i--)
            {
                cache[i] = cache[i - 1];
                if (cache[i] > 0.001f)
                {
                    ping += cache[i];
                }
            }
            ping /= cache.Length;
            cache[0] = currentPing;
        }
    }
}