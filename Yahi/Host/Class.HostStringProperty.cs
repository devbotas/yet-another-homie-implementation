using System;

namespace DevBot9.Protocols.Homie {
    public class HostStringProperty : HostPropertyBase {
        public string Value {
            get {
                return _rawValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostStringProperty(PropertyType propertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyType, propertyId, friendlyName, dataType, format, unit) {
            _rawValue = "";
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }
        protected override bool ValidatePayload(string payloadToValidate) {
            return true;
        }

        private void SetValue(string valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    _rawValue = valueToSet;

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
