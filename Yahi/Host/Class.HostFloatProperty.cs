using System;
using System.Globalization;

namespace DevBot9.Protocols.Homie {
    public class HostFloatProperty : HostPropertyBase {
        public float Value {
            get {
                float returnValue;

                returnValue = float.Parse(_rawValue, CultureInfo.InvariantCulture);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostFloatProperty(PropertyType propertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyType, propertyId, friendlyName, dataType, format, unit) {
            _rawValue = "0.0";
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = float.TryParse(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:

                    _rawValue = valueToSet.ToString("0.0#", CultureInfo.InvariantCulture);

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
