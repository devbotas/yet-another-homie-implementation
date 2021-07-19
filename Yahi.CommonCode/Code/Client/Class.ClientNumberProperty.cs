using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class ClientNumberProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public float Value {
            get {
                float returnValue;

                if (_isLegacyInteger) {
                    returnValue = int.Parse(_rawValue);
                }
                else {
                    returnValue = Helpers.ParseFloat(_rawValue);
                }

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        bool _isLegacyInteger = false;

        internal ClientNumberProperty(ClientPropertyMetadata creationOptions, bool isLegacyInteger = false) : base(creationOptions) {
            _isLegacyInteger = isLegacyInteger;
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            bool isPayloadValid;

            if (_isLegacyInteger) {
                isPayloadValid = Helpers.TryParseInt(payloadToValidate, out _);
            }
            else {
                isPayloadValid = Helpers.TryParseFloat(payloadToValidate, out _);
            }

            return isPayloadValid;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    if (_isLegacyInteger) {
                        _rawValue = valueToSet.ToString();
                    }
                    else {
                        _rawValue = Helpers.FloatToString(valueToSet, Format);
                    }

                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
