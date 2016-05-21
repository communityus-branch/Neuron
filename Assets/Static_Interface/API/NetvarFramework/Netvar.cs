using System;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.SerialisationFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;

namespace Static_Interface.API.NetvarFramework
{
    public abstract class Netvar
    {
        protected Netvar()
        {
            OnPreInit();
            Value = GetDefaultValue();
        }

        protected virtual void OnPreInit()
        {
            
        }

        public abstract string Name { get; }

        public abstract object GetDefaultValue();

        public virtual Type GetValueType()
        {
            if (GetDefaultValue() == null)
            {
                throw new ArgumentException("Netvar \"" + Name + "\": GetDefaultValue == null, GetValueType should be overwritten");
            }

            return GetDefaultValue().GetType();
        }

        private object _value;
        public object Value
        {
            get { return OnGetValue(); }
            set
            {
                if ((_value == null && value == null) || (_value != null && _value.Equals(value))) return;
                ValidateType(value, true);
                NetvarChangedEvent @event = new NetvarChangedEvent(this, _value, value);
                if (@event.IsCancelled)
                {
                    return;
                }
                OnSetValue(_value, value);
                LogUtils.Log("Netvar \"" + Name + "\" updated:" + @event.OldValue + " -> " + @event.NewValue);
                _value = value;
                SendNetvarUpdate();
            }
        }

        private void SendNetvarUpdate()
        {
            if (!Connection.IsServer()) return;
            NetvarManager.Instance.Channel.Send("Network_ReceiveValueUpdate", ECall.Clients, Name, Serialize());
        }

        public object Deserialize(byte[] serializedData)
        {
            if (serializedData.Length == 0) return null;
            return ObjectSerializer.GetObjects(0, 0, serializedData, GetValueType())[0];
        }

        public bool ValidateType(object t, bool throwException = false)
        {
            if (t == null)
            {
                if (IsNullable(GetValueType()))
                {
                    return true;
                }
                if (throwException) throw new ArgumentException("Type " + GetValueType().FullName + " cannot be null");
                return false;
            }

            if (t.GetType() == GetValueType() || GetDefaultValue().GetType().IsInstanceOfType(t)) return true;
            if(throwException) throw new InvalidCastException("Cannot cast " + t.GetType().FullName + " to type "  + GetValueType().FullName);
            return false;
        }

        private bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        protected virtual void OnSetValue(object oldValue, object newValue)
        {

        }

        public virtual T GetParsedValue<T>()
        {
            return (T) Value;
        }

        protected virtual object OnGetValue()
        {
            return _value;
        }

        public byte[] Serialize()
        {
            if (Value == null) return new byte[0];
            int size;
            var data = ObjectSerializer.GetBytes(0, out size, Value);
            return data;
        }
    }
}