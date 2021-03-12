using System;

namespace DevBot9.Protocols.Homie {
    public class HostIntegerProperty : HostPropertyBase {
        public int Value {
            get {
                int returnValue;

                returnValue = int.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostIntegerProperty(PropertyType propertyType, string propertyId, string friendlyName, DataType dataType, int initialValue, string format, string unit) : base(propertyType, propertyId, friendlyName, dataType, format, unit) {
            _rawValue = initialValue.ToString();
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = int.TryParse(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(int valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:

                    _rawValue = valueToSet.ToString();

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
