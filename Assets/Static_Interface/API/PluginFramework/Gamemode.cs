namespace Static_Interface.API.PluginFramework
{
    public abstract class GameMode : Plugin
    {
        public static GameMode CurrentGameMode { get; internal set; }
        public abstract string Description { get; }
    }
}