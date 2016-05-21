using System;

namespace Static_Interface.API.SerializationFramework
{
    public abstract class Serializer<T> : ISerializer
    {
        public abstract byte[] Serialize(T obj);

        protected abstract T Deserialize(byte[] data);

        byte[] ISerializer.Serialize(object o)
        {
            return Serialize((T)o);
        }

        object ISerializer.Deserialize(DataBuffer data)
        {
            return OnPreRead(data);
        }

        public Type GetSerializableType()
        {
            return typeof (T);
        }

        public T Deserialize(DataBuffer data)
        {
            return OnPreRead(data);
        }

        protected virtual T OnPreRead(DataBuffer data)
        {
            var length  = GetLength(data);
            byte[] dataRead = data.ReadBytes(length);
            return Deserialize(dataRead);
        }

        public abstract int GetLength(DataBuffer buffer);
    }

    public interface ISerializer
    {
        int GetLength(DataBuffer buffer);
        byte[] Serialize(object o);
        object Deserialize(DataBuffer data);
        Type GetSerializableType();
    }
}