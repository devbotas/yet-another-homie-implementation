namespace DevBot9.Protocols.Homie {
    public class HostParameterProperty : HostPropertyBase {
        internal HostParameterProperty(string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, true, true, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);

            _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (value) => {
                Value.SetValue(value);
                _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.GetStringValue());
            });
        }
        public void SetValue(string valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends a new parameter value.
            _value.SetValue(valueToSet);
            _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.GetStringValue());
        }
    }
}
