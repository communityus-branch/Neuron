namespace Static_Interface.Utils
{
    public static class ReflectionUtils
    {
        public static T CastTo<T>(this object generiucValue) where T: class
        {
            return (T) generiucValue;
        }
    }
}