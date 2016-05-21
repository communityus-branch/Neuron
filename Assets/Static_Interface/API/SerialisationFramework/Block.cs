using System;
using System.Text;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.SerialisationFramework
{
    public class Block
    {
        private byte[] _block;
        public static readonly byte[] Shifts = { 1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80 };

        public const int BUFFER_SIZE = 0x8000;
        private static readonly byte[] Buffer = new byte[BUFFER_SIZE];

        private int _step;

        public Block()
        {
            Reset();
        }

        public Block(byte[] contents)
        {
            Reset(contents);
        }

        public Block(int prefix)
        {
            Reset(prefix);
        }

        public Block(int prefix, byte[] contents)
        {
            Reset(prefix, contents);
        }

        public byte[] GetBytes(out int size)
        {
            if (_block == null)
            {
                size = _step;
                var tmp = new byte[size];
                System.Buffer.BlockCopy(Buffer, 0, tmp, 0, _step);
                return tmp;
            }
            size = _block.Length;
            return _block;
        }

        public byte[] GetHash()
        {
            if (_block == null)
            {
                return Hash.SHA1(Buffer);
            }
            return Hash.SHA1(_block);
        }

        public object Read(Type type)
        {
            if (type == typeof(string))
            {
                return ReadString();
            }
            if (type == typeof(bool))
            {
                return ReadBoolean();
            }
            if (type == typeof(bool[]))
            {
                return ReadBooleanArray();
            }
            if (type == typeof(byte))
            {
                return ReadByte();
            }
            if (type == typeof(byte[]))
            {
                return ReadByteArray();
            }
            if (type == typeof(short))
            {
                return ReadInt16();
            }
            if (type == typeof(ushort))
            {
                return ReadUInt16();
            }
            if (type == typeof(int))
            {
                return ReadInt32();
            }
            if (type == typeof(int[]))
            {
                return ReadInt32Array();
            }
            if (type == typeof(uint))
            {
                return ReadUInt32();
            }
            if (type == typeof(float))
            {
                return ReadSingle();
            }
            if (type == typeof(long))
            {
                return ReadInt64();
            }
            if (type == typeof(ulong))
            {
                return ReadUInt64();
            }
            if (type == typeof(ulong[]))
            {
                return ReadUInt64Array();
            }
            if (type == typeof(Vector3))
            {
                return ReadSingleVector3();
            }
            if (type == typeof(Color))
            {
                return ReadColor();
            }
            if (type == typeof(Identity)|| type.IsSubclassOf(typeof(Identity)))
            {
                return ReadIdentity();
            }
            if (type == typeof (Quaternion))
            {
                return Quaternion.Euler(ReadSingleVector3());
            }
            LogUtils.LogError("Failed to read type: " + type);
            return null;
        }

        private Identity ReadIdentity()
        {
            ulong asd = ReadUInt64();
            return Connection.CurrentConnection.Provider.Deserialilze(asd);
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

        public bool ReadBoolean()
        {
            if ((_block != null) && (_step <= (_block.Length - 1)))
            {
                bool flag = BitConverter.ToBoolean(_block, _step);
                _step += sizeof(bool);
                return flag;
            }
            return false;
        }

        public bool[] ReadBooleanArray()
        {
            if ((_block == null) || (_step >= _block.Length))
            {
                return new bool[0];
            }
            bool[] flagArray = new bool[ReadUInt16()];
            ushort num = (ushort) Mathf.CeilToInt(flagArray.Length/8f);
            for (ushort i = 0; i < num; i = (ushort) (i + 1))
            {
                for (byte j = 0; j < 8; j = (byte) (j + 1))
                {
                    if (((i*8) + j) >= flagArray.Length)
                    {
                        break;
                    }
                    flagArray[(i*8) + j] = (_block[_step + i] & Shifts[j]) == Shifts[j];
                }
            }
            _step += num;
            return flagArray;
        }

        public byte ReadByte()
        {
            if ((_block != null) && (_step <= (_block.Length - 1)))
            {
                byte num = _block[_step];
                _step += sizeof(byte);
                return num;
            }
            return 0;
        }

        public byte[] ReadByteArray()
        {
            if ((_block == null) || (_step >= _block.Length))
            {
                return new byte[0];
            }
            byte[] dst = new byte[_block[_step]];
            _step++;
            try
            {
                System.Buffer.BlockCopy(_block, _step, dst, 0, dst.Length);
            }
            catch (Exception)
            {
                // ignored
            }
            _step += dst.Length;
            return dst;
        }

        public Color ReadColor()
        {
            return new Color((ReadByte())/255f, (ReadByte())/255f,
                (ReadByte())/255f);
        }

        public short ReadInt16()
        {
            if ((_block == null) || (_step > (_block.Length - 2))) return 0;
            var num = BitConverter.ToInt16(_block, _step);
            _step += sizeof(short);
            return num;
        }

        public int ReadInt32()
        {
            if ((_block == null) || (_step > (_block.Length - 4))) return 0;
            var num = BitConverter.ToInt32(_block, _step);
            _step += sizeof(int);
            return num;
        }

        public int[] ReadInt32Array()
        {
            var num = ReadUInt16();
            var numArray = new int[num];
            for (ushort i = 0; i < num; i = (ushort) (i + 1))
            {
                numArray[i] = ReadInt32();
            }
            return numArray;
        }

        public long ReadInt64()
        {
            if ((_block == null) || (_step > (_block.Length - 8))) return 0L;
            var num = BitConverter.ToInt64(_block, _step);
            _step += sizeof(long);
            return num;
        }

        public float ReadSingle()
        {
            if ((_block == null) || (_step > (_block.Length - 4))) return 0f;
            var num = BitConverter.ToSingle(_block, _step);
            _step += sizeof(float);
            return num;
        }

        public Quaternion ReadSingleQuaternion()
        {
            return Quaternion.Euler(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Vector3 ReadSingleVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public string ReadString()
        {
            if ((_block != null) && (_step < _block.Length))
            {
                string str = Encoding.UTF8.GetString(_block, _step + 1, _block[_step]);
                _step = (_step + 1) + _block[_step];
                return str;
            }
            return string.Empty;
        }

        public ushort ReadUInt16()
        {
            if ((_block != null) && (_step <= (_block.Length - 2)))
            {
                ushort num = BitConverter.ToUInt16(_block, _step);
                _step += 2;
                return num;
            }
            return 0;
        }

        public uint ReadUInt32()
        {
            if ((_block != null) && (_step <= (_block.Length - 4)))
            {
                uint num = BitConverter.ToUInt32(_block, _step);
                _step += 4;
                return num;
            }
            return 0;
        }

        public ulong ReadUInt64()
        {
            if ((_block != null) && (_step <= (_block.Length - 8)))
            {
                ulong num = BitConverter.ToUInt64(_block, _step);
                _step += 8;
                return num;
            }
            return 0L;
        }

        public ulong[] ReadUInt64Array()
        {
            ushort num = ReadUInt16();
            ulong[] numArray = new ulong[num];
            for (ushort i = 0; i < num; i = (ushort) (i + 1))
            {
                numArray[i] = ReadUInt64();
            }
            return numArray;
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

        public void Write(object objects)
        {
            if(objects == null) throw new ArgumentNullException(nameof(objects));

            Type type = objects.GetType();
            if (type == typeof(String))
            {
                WriteString((string) objects);
            }
            else if (type == typeof(bool))
            {
                WriteBoolean((bool) objects);
            }
            else if (type == typeof(bool[]))
            {
                WriteBooleanArray((bool[]) objects);
            }
            else if (type == typeof(byte))
            {
                WriteByte((byte) objects);
            }
            else if (type == typeof(byte[]))
            {
                WriteByteArray((byte[]) objects);
            }
            else if (type == typeof(short))
            {
                WriteInt16((short) objects);
            }
            else if (type == typeof(ushort))
            {
                WriteUInt16((ushort) objects);
            }
            else if (type == typeof(int))
            {
                WriteInt32((int) objects);
            }
            else if (type == typeof(int[]))
            {
                WriteInt32Array((int[]) objects);
            }
            else if (type == typeof(uint))
            {
                WriteUInt32((uint) objects);
            }
            else if (type == typeof(float))
            {
                WriteSingle((float) objects);
            }
            else if (type == typeof(long))
            {
                WriteInt64((long) objects);
            }
            else if (type == typeof(ulong))
            {
                WriteUInt64((ulong) objects);
            }
            else if (type == typeof(ulong[]))
            {
                WriteUInt64Array((ulong[]) objects);
            }
            else if (type == typeof(Vector3))
            {
                WriteSingleVector3((Vector3) objects);
            }
            else if (type == typeof(Color))
            {
                WriteColor((Color) objects);
            } else if (type == typeof(Identity) || type.IsSubclassOf(typeof(Identity)))
            {
                WriteUInt64(((Identity)objects).Serialize());
            } else if (type == typeof (Quaternion))
            {
                WriteSingleVector3(((Quaternion)objects).eulerAngles);
            }
            else
            {
                LogUtils.LogError("Failed to write type: " + type + " (type not supported)");
            }
        }

        public void Write(params object[] objects)
        {
            foreach (var obj in objects)
            {
                Write(obj);
            }
        }

        public void WriteBoolean(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer[_step] = bytes[0];
            _step+= sizeof(byte);
        }

        public void WriteBooleanArray(bool[] values)
        {
            WriteUInt16((ushort) values.Length);
            ushort num = (ushort) Mathf.CeilToInt((values.Length)/8f);
            for (ushort i = 0; i < num; i = (ushort) (i + 1))
            {
                Buffer[_step + i] = 0;
                for (byte j = 0; j < 8; j = (byte) (j + 1))
                {
                    if (((i*8) + j) >= values.Length)
                    {
                        break;
                    }
                    if (values[(i*8) + j])
                    {
                        Buffer[_step + i] = (byte) (Buffer[_step + i] | Shifts[j]);
                    }
                }
            }
            _step += num;
        }

        public void WriteByte(byte value)
        {
            Buffer[_step] = value;
            _step+=sizeof(byte);
        }

        public void WriteByteArray(byte[] values)
        {
            Buffer[_step] = (byte) values.Length;
            _step++;
            System.Buffer.BlockCopy(values, 0, Buffer, _step, values.Length);
            _step += values.Length;
        }

        public void WriteColor(Color value)
        {
            WriteByte((byte) (value.r*255f));
            WriteByte((byte) (value.g*255f));
            WriteByte((byte) (value.b*255f));
        }

        public void WriteInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(short);
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(int);
        }

        public void WriteInt32Array(int[] values)
        {
            WriteUInt16((ushort) values.Length);
            for (ushort i = 0; i < values.Length; i = (ushort) (i + 1))
            {
                WriteInt32(values[i]);
            }
        }

        public void WriteInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(long);
        }

        public void WriteSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(float);
        }

        public void WriteSingleQuaternion(Quaternion value)
        {
            Vector3 eulerAngles = value.eulerAngles;
            WriteSingle(eulerAngles.x);
            WriteSingle(eulerAngles.y);
            WriteSingle(eulerAngles.z);
        }

        public void WriteSingleVector3(Vector3 value)
        {
            WriteSingle(value.x);
            WriteSingle(value.y);
            WriteSingle(value.z);
        }

        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Buffer[_step] = (byte) bytes.Length;
            _step++;
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += bytes.Length;
        }

        public void WriteUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(ushort);
        }

        public void WriteUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(uint);
        }

        public void WriteUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += sizeof(ulong);
        }

        public void WriteUInt64Array(ulong[] values)
        {
            WriteUInt16((ushort) values.Length);
            for (ushort i = 0; i < values.Length; i = (ushort) (i + 1))
            {
                WriteUInt64(values[i]);
            }
        }
    }
}

