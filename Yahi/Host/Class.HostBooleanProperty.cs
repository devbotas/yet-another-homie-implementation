using System;

namespace DevBot9.Protocols.Homie {
    public class HostBooleanProperty : HostPropertyBase {
        public bool Value {
            get {
                bool returnValue;

                returnValue = bool.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostBooleanProperty(PropertyType protertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(protertyType, propertyId, friendlyName, dataType, format, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = false;

            if ((payloadToValidate == "true") || (payloadToValidate == "false")) {
                returnValue = true;
            }

            return returnValue;
        }

        private void SetValue(bool valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    if (valueToSet == true) {
                        _rawValue = "true";
                    }
                    else {
                        _rawValue = "false";
                    }

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
