namespace DevBot9.Protocols.Homie {
    public class HostStateProperty : HostPropertyBase {
        internal HostStateProperty(string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        public void SetValue(string valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends an external command.
            _value = valueToSet;

            _parentDevice.InternalPropertyPublish($"{_propertyId}", Value);
        }
    }
}
