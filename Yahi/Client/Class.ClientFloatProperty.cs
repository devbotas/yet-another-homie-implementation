using System;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class ClientFloatProperty : ClientPropertyBase {
        public PropertyType Type = PropertyType.State;

        public float Value {
            get {
                float returnValue;
                returnValue = float.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientFloatProperty(PropertyType protertyType, string propertyId) : base(propertyId) {
            _rawValue = "0.0";
            Type = protertyType;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
            if (Type == PropertyType.State) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
                    }
                });
            }

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
                    }
                });
            }
        }

        private bool ValidatePayload(string payloadToValidate) {
            var returnValue = float.TryParse(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = valueToSet.ToString();
                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
