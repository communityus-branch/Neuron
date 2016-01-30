using Static_Interface.API.Event;

namespace Static_Interface.API.Netvar
{
    public class NetvarChangedEvent: Event.Event, ICancellable
    {
        public bool IsCancelled { get; set; }
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