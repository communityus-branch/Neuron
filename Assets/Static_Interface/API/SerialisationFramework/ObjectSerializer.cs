using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SerialisationFramework.Impl;

namespace Static_Interface.API.SerialisationFramework
{
    public static class ObjectSerializer
    {

        public static byte[] GetBytes(int prefix, params object[] objects)
        {
            DataBuffer tmp = new DataBuffer(prefix);
            foreach(object o in objects) 
                tmp.Write(o);
            return tmp.GetBytes();
        }

        public static object[] GetObjects(int offset, int prefix, byte[] bytes, params Type[] types)
        {
            return GetObjects(null, offset, prefix, bytes, false, types);
        }

        public static object[] GetObjects(Identity ident, int offset, int prefix, byte[] bytes, bool isChannel, params Type[] types)
        {
            DataBuffer buffer = new DataBuffer(offset + prefix, bytes);
            if (!isChannel) return buffer.Read(types);

            object[] objArray = buffer.Read(1, types);
            objArray[0] = ident;
            return objArray;
        }

        private static readonly List<ISerializer> Serializers = new List<ISerializer>();  
        public static void RegisterSerializer<T>(Serializer<T> serializer)
        {
            if(GetSerializer<T>(false) != null)
                throw new Exception("A serializer for this Type already exists!");

            Serializers.Add(serializer);
        }

        internal static void ResetSerializers()
        {
            Serializers.Clear();
            FirstTimeInit();
        }

        internal static void FirstTimeInit()
        {
            RegisterSerializer(new BoolSerializer());
            RegisterSerializer(new ByteSerializer());
            RegisterSerializer(new ColorSerializer());
            RegisterSerializer(new DecimalSerializer());
            RegisterSerializer(new DoubleSerializer());
            RegisterSerializer(new IdentitySerializer());
            RegisterSerializer(new Int16Serializer());
            RegisterSerializer(new Int32Serializer());
            RegisterSerializer(new Int64Serializer());
            RegisterSerializer(new QuaternionSerializer());
            RegisterSerializer(new SignedByteSerializer());
            RegisterSerializer(new SingleSerializer());
            RegisterSerializer(new UnicodeStringSerializer());
            RegisterSerializer(new UnsignedInt16Serializer());
            RegisterSerializer(new UnsignedInt32Serializer());
            RegisterSerializer(new UnsignedInt64Serializer());
            RegisterSerializer(new Vector2Serializer());
            RegisterSerializer(new Vector3Serializer());
            RegisterSerializer(new Vector4Serializer());
        }

        public static Serializer<T> GetSerializer<T>(bool throwExceptionOnNotFound = true)
        {
            var serializer  = Serializers.Where(s => s.GetSerializableType() == typeof (T) || s.GetSerializableType().IsAssignableFrom(typeof(T))).Cast<Serializer<T>>().FirstOrDefault();
            if (serializer == null && throwExceptionOnNotFound)
                throw new Exception($"Serializer for type {typeof(T).FullName} not found");
            return serializer;
        }

        public static ISerializer GetSerializer(Type t, bool throwExceptionOnNotFound = true)
        {
            if(Serializers.Count == 0)
                FirstTimeInit();
            var serializer = Serializers.FirstOrDefault(s => s.GetSerializableType() == t || s.GetSerializableType().IsAssignableFrom(t));
            if(serializer == null && throwExceptionOnNotFound)
                throw new Exception($"Serializer for type {t.FullName} not found");
            return serializer;
        } 
    }
}
