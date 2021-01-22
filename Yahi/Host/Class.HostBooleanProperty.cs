using System;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class HostBooleanProperty : HostPropertyBase {
        public PropertyType Type = PropertyType.State;

        public new bool Value {
            get {
                bool returnValue;

                returnValue = bool.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostBooleanProperty(PropertyType protertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
            Type = protertyType;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));

                        _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.ToString());
                    }
                });
            }

            if (Type == PropertyType.Command) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
                    }
                });
            }
        }

        private bool ValidatePayload(string payloadToValidate) {
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
