using Steamworks;
using System;
using System.Text;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.Internal.Objects
{
    public class Block
    {
        private byte[] _block;

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
                return Buffer;
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
            if (type == Types.STRING_TYPE)
            {
                return ReadString();
            }
            if (type == Types.BOOLEAN_TYPE)
            {
                return ReadBoolean();
            }
            if (type == Types.BOOLEAN_ARRAY_TYPE)
            {
                return ReadBooleanArray();
            }
            if (type == Types.BYTE_TYPE)
            {
                return ReadByte();
            }
            if (type == Types.BYTE_ARRAY_TYPE)
            {
                return ReadByteArray();
            }
            if (type == Types.INT16_TYPE)
            {
                return ReadInt16();
            }
            if (type == Types.UINT16_TYPE)
            {
                return ReadUInt16();
            }
            if (type == Types.INT32_TYPE)
            {
                return ReadInt32();
            }
            if (type == Types.INT32_ARRAY_TYPE)
            {
                return ReadInt32Array();
            }
            if (type == Types.UINT32_TYPE)
            {
                return ReadUInt32();
            }
            if (type == Types.SINGLE_TYPE)
            {
                return ReadSingle();
            }
            if (type == Types.INT64_TYPE)
            {
                return ReadInt64();
            }
            if (type == Types.UINT64_TYPE)
            {
                return ReadUInt64();
            }
            if (type == Types.UINT64_ARRAY_TYPE)
            {
                return ReadUInt64Array();
            }
            if (type == Types.STEAM_ID_TYPE)
            {
                return ReadSteamID();
            }
            if (type == Types.VECTOR3_TYPE)
            {
                return ReadSingleVector3();
            }
            if (type == Types.KEYSTATE_TYPE)
            {
                return ReadKeyState();
            }
            if (type == Types.COLOR_TYPE)
            {
                return ReadColor();
            }
            if (type == Types.IDENTITY_TYPE || type.IsSubclassOf(Types.IDENTITY_TYPE))
            {
                return ReadIdentity();
            }
            LogUtils.LogError("Failed to read type: " + type);
            return null;
        }

        private Identity ReadIdentity()
        {
            ulong asd = ReadUInt64();
            LogUtils.Debug("Reading identity: " + asd);
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
                _step++;
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
                    flagArray[(i*8) + j] = (_block[_step + i] & Types.SHIFTS[j]) == Types.SHIFTS[j];
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
                _step++;
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
            _step += 2;
            return num;
        }

        public int ReadInt32()
        {
            if ((_block == null) || (_step > (_block.Length - 4))) return 0;
            var num = BitConverter.ToInt32(_block, _step);
            _step += 4;
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
            _step += 8;
            return num;
        }

        public float ReadSingle()
        {
            if ((_block == null) || (_step > (_block.Length - 4))) return 0f;
            var num = BitConverter.ToSingle(_block, _step);
            _step += 4;
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

        public KeyState ReadKeyState()
        {
            return new KeyState {KeyCode = ReadInt32(), IsDown = ReadBoolean(), IsPressed = ReadBoolean()};
        }

        public CSteamID ReadSteamID()
        {
            return new CSteamID(ReadUInt64());
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
            Type type = objects.GetType();
            if (type == Types.STRING_TYPE)
            {
                WriteString((string) objects);
            }
            else if (type == Types.BOOLEAN_TYPE)
            {
                WriteBoolean((bool) objects);
            }
            else if (type == Types.BOOLEAN_ARRAY_TYPE)
            {
                WriteBooleanArray((bool[]) objects);
            }
            else if (type == Types.BYTE_TYPE)
            {
                WriteByte((byte) objects);
            }
            else if (type == Types.BYTE_ARRAY_TYPE)
            {
                WriteByteArray((byte[]) objects);
            }
            else if (type == Types.INT16_TYPE)
            {
                WriteInt16((short) objects);
            }
            else if (type == Types.UINT16_TYPE)
            {
                WriteUInt16((ushort) objects);
            }
            else if (type == Types.INT32_TYPE)
            {
                WriteInt32((int) objects);
            }
            else if (type == Types.INT32_ARRAY_TYPE)
            {
                WriteInt32Array((int[]) objects);
            }
            else if (type == Types.UINT32_TYPE)
            {
                WriteUInt32((uint) objects);
            }
            else if (type == Types.SINGLE_TYPE)
            {
                WriteSingle((float) objects);
            }
            else if (type == Types.INT64_TYPE)
            {
                WriteInt64((long) objects);
            }
            else if (type == Types.UINT64_TYPE)
            {
                WriteUInt64((ulong) objects);
            }
            else if (type == Types.UINT64_ARRAY_TYPE)
            {
                WriteUInt64Array((ulong[]) objects);
            }
            else if (type == Types.STEAM_ID_TYPE)
            {
                WriteSteamID((CSteamID) objects);
            }
            else if (type == Types.VECTOR3_TYPE)
            {
                WriteSingleVector3((Vector3) objects);
            }
            else if (type == Types.KEYSTATE_TYPE)
            {
                WriteKeyState((KeyState) objects);
            }
            else if (type == Types.COLOR_TYPE)
            {
                WriteColor((Color) objects);
            } else if (type ==Types.IDENTITY_TYPE || type.IsSubclassOf(Types.IDENTITY_TYPE))
            {
                LogUtils.Debug("Writing identity: " + ((Identity)objects).Serialize());
                WriteUInt64(((Identity)objects).Serialize());
            }
            else
            {
                LogUtils.LogError("Failed to write type: " + type);
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
            _step++;
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
                        Buffer[_step + i] = (byte) (Buffer[_step + i] | Types.SHIFTS[j]);
                    }
                }
            }
            _step += num;
        }

        public void WriteByte(byte value)
        {
            Buffer[_step] = value;
            _step++;
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
            _step += 2;
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += 4;
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
            _step += 8;
        }

        public void WriteSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += 4;
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

        public void WriteKeyState(KeyState value)
        {
            WriteInt32(value.KeyCode);
            WriteBoolean(value.IsDown);
            WriteBoolean(value.IsPressed);
        }

        public void WriteSteamID(CSteamID steamID)
        {
            WriteUInt64(steamID.m_SteamID);
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
            _step += 2;
        }

        public void WriteUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += 4;
        }

        public void WriteUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, Buffer, _step, bytes.Length);
            _step += 8;
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

