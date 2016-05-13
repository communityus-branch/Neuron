using System;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.Objects;

namespace Static_Interface.API.NetvarFramework
{
    public abstract class Netvar : NetworkedBehaviour
    {
        protected sealed override void Awake()
        {
            base.Awake();
            NetvarManager.Instance.RegisterNetvar(this);
            OnInit();
        }

        protected virtual void OnInit()
        {
            
        }

        public abstract string Name { get; }

        public abstract object GetDefaultValue();

        public virtual Type GetValueType()
        {
            if (GetDefaultValue() == null)
            {
                throw new ArgumentException("GetDefaultValue == null, GetValueType should be overwritten");
            }

            return GetDefaultValue().GetType();
        }

        private object _value;
        public object Value
        {
            get { return OnGetValue(); }
            set
            {
                ValidateType(value, true);
                NetvarChangedEvent @event = new NetvarChangedEvent(this, _value, value);
                if (@event.IsCancelled)
                {
                    return;
                }
                OnSetValue(_value, value);
                LogUtils.Log("Netvar \"" + @event.Name + "\" updated:" + @event.OldValue + " -> " + @event.NewValue);
                _value = value;
                SendNetvarUpdate();
            }
        }

        private void SendNetvarUpdate()
        {
            if (!IsServer()) return;
            byte[] serializedData = Serialize();
            Channel.Send(nameof(Network_ReceiveValueUpdate), ECall.Clients, serializedData);
        }



        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        public void Network_ReceiveValueUpdate(Identity ident, byte[] serializedData)
        {
            Value = Deserialize(serializedData);
        }

        public byte[] Serialize()
        {
            int size;
            return ObjectSerializer.GetBytes(0, out size, Value);
        }

        public object Deserialize(byte[] serializedData)
        {
            return ObjectSerializer.GetObjects(0, 0, serializedData, GetValueType());
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
    }
}