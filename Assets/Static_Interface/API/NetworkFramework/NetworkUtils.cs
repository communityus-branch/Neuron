using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework;

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

        public static int GetNextChannelID()
        {
            return Connection.CurrentConnection.ChannelCount +1;
        }

        public static bool IsServer()
        {
            return Connection.IsServer();
        }

        public static bool IsDedicated()
        {
            return Connection.IsDedicated;
        }

        public static bool IsClient()
        {
            return Connection.IsClient();
        }

        public static Identity MyIdent => Connection.CurrentConnection.ClientID;
        public static Identity ServerIdent => Connection.CurrentConnection.ServerID;
        public static List<Player> Players => Connection.CurrentConnection.Clients.Select(c => c.Player).ToList();
    }
}