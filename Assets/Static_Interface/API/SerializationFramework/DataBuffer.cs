using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.Utils;

namespace Static_Interface.API.SerializationFramework
{
    public class DataBuffer
    {
        private byte[] _block;

        public const int BUFFER_SIZE = 0x8000;
        private readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private int _step;

        public DataBuffer()
        {
            Reset();
        }

        public DataBuffer(byte[] contents)
        {
            Reset(contents);
        }

        public DataBuffer(int prefix)
        {
            Reset(prefix);
        }

        public DataBuffer(int prefix, byte[] contents)
        {
            Reset(prefix, contents);
        }

        public byte[] GetBytes()
        {
            if (_block == null)
            {
                var tmp = new byte[_step];
                System.Buffer.BlockCopy(_buffer, 0, tmp, 0, _step);
                return tmp;
            }
            return _block;
        }

        public byte[] GetHash()
        {
            if (_block == null)
            {
                return Hash.SHA1(_buffer);
            }
            return Hash.SHA1(_block);
        }

        public T Read<T>()
        {
            return (T)Read(typeof(T));
        }

        public object Read(Type type)
        {
            if ((_block == null)) return null;

            ISerializer serializer;
            if (type.IsArray)
            {
                serializer = ObjectSerializer.GetSerializer(type.GetElementType());
                int lenght = Read<int>();
                var arr = Array.CreateInstance(type.GetElementType(), lenght);
                for (int i = 0; i < arr.Length; i++)
                {
                    var deserialized = serializer.Deserialize(this);
                    arr.SetValue(deserialized, i);
                }
                return arr;
            }
  
            serializer =  ObjectSerializer.GetSerializer(type);
            return serializer.Deserialize(this);
        }
        
        public byte ReadByte()
        {
            if (_block == null)
                throw new NullReferenceException("DataBuffer has not content!");

            if ((_step > (_block.Length - 1)))
                throw new BufferOverflowException(_block.Length, _step, 1);

            byte num = _block[_step];
            _step += sizeof(byte);
            return num;
        }
        
        public byte[] ReadBytes(int length)
        {
            if ((_block == null))
                throw new NullReferenceException("DataBuffer has not content!");
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = ReadByte();
            }
            return data;
        }

        public object[] Read(params Type[] types)
        {
            return Read(0, types);
        }

        public object[] Read(int offset, params Type[] types)
        {
            object[] objArray = new object[types.Length];
            for (int i = offset; i < types.Length; i++)
            {
                objArray[i] = Read(types[i]);
            }
            return objArray;
        }
        
        public void Reset()
        {
            _step = 0;
            _block = null;
        }

        public void Reset(byte[] contents)
        {
            _step = 0;
            _block = contents;
        }

        public void Reset(int prefix)
        {
            _step = prefix;
            _block = null;
        }

        public void Reset(int prefix, byte[] contents)
        {
            _step = prefix;
            _block = contents;
        }

        public void Write(object @object)
        {
            if(@object == null) throw new ArgumentNullException(nameof(@object));
            Type type = @object.GetType();
            ISerializer serializer;
            byte[] totalData;
            if (type.IsArray)
            {
                serializer = ObjectSerializer.GetSerializer(type.GetElementType());
                int length = ((Array) @object).Length;
                List<byte> data = BitConverter.GetBytes(length).ToList();

                for (int i = 0; i < length; i++)
                {
                    var bytes = serializer.Serialize(((Array)@object).GetValue(i));
                    data.AddRange(bytes);
                }

                totalData = data.ToArray();
            }
            else
            {
                serializer = ObjectSerializer.GetSerializer(type);
                totalData = serializer.Serialize(@object);
            }

            System.Buffer.BlockCopy(totalData, 0, _buffer, _step, totalData.Length);
            _step += totalData.Length;
        }

        public void Write(params object[] objects)
        {
            foreach (var obj in objects)
            {
                Write(obj);
            }
        }
    }

    public class BufferOverflowException : Exception
    {
        public BufferOverflowException(int size, int pos, int length) : base($"Trying to read more bytes than available (buffer size: {size} buffer pos: {pos} bytes tried to read: {length})")
        {
            
        }
    }
}

