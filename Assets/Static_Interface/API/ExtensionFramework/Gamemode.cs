namespace Static_Interface.API.ExtensionFramework
{
    public abstract class GameMode : Extension
    {
        public static GameMode CurrentGameMode { get; internal set; }
        public abstract string Description { get; }
    }
}