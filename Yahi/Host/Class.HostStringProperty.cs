using System;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class HostStringProperty : HostPropertyBase {
        public PropertyType Type = PropertyType.State;

        public string Value {
            get {
                return _rawValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostStringProperty(PropertyType protertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
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
