using System;
using Static_Interface.API.EventFramework;
using Static_Interface.API.Netvar;

namespace Assets.Events
{
    public class NetvarChangedEvent: Event, ICancellable
    {
        private bool _cancelled;
        public bool IsCancelled()
        {
            return _cancelled;
        }
        public void SetCancelled(bool cancelled)
        {
            _cancelled = cancelled;
        }

        public Netvar Netvar;
        public object OldValue;
        public object NewValue;

        public NetvarChangedEvent(Netvar netvar, object oldvalue, object newvalue) : base(false)
        {
            Netvar = netvar;
            OldValue= oldvalue;
            NewValue = newvalue;
        }
    }
}