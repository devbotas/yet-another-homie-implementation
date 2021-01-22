namespace DevBot9.Protocols.Homie {
    public class HostNumericProperty : HostPropertyBase {
        public PropertyType Type = PropertyType.State;

        public new HomieNumber Value {
            get {
                return HomieNumber;
            }
            set {
                if (Type != PropertyType.Command) {

                    HomieNumber = value;

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
                }
            }
        }

        internal HostNumericProperty(PropertyType protertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
            Type = protertyType;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (value) => {
                    double.TryParse(value, out var newDouble);
                    Value = newDouble;
                    _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
                });
            }

            if (Type == PropertyType.Command) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}", (value) => {
                    double.TryParse(value, out var newDouble);
                    Value = newDouble;
                });
            }
        }

        public void SetValue(int valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends an external command.
            _homieNumber = valueToSet;

            //_parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
        }
        public void SetValue(double valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends an external command.
            _homieNumber = valueToSet;

            // _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
        }
    }
}
