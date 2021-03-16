using System;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Boolean, as defined by the Homie convention.
    /// </summary>
    public class ClientBooleanProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
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

        internal ClientBooleanProperty(PropertyType propertyType, string propertyId) : base(propertyId) {
            Type = propertyType;
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

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = false;

            if ((payloadToValidate == "true") || (payloadToValidate == "false")) {
                returnValue = true;
            }

            return returnValue;
        }

        private void SetValue(bool valueToSet) {
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
