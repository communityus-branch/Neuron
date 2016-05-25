namespace Static_Interface.API.CommandFramework
{
    public interface ICommandSender
    {
        string Name { get; }
        bool HasPermission(string permission);
        void Message(string msg);
        string CommandPrefix { get; }
    }
}