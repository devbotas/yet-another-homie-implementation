namespace DevBot9.Protocols.Homie {
    public class HostStringStateProperty : HostPropertyBase {
        public new string Value {
            get {
                return HomieString;
            }
            set {
                HomieString = value;

                _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
            }
        }

        internal HostStringStateProperty(string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
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
