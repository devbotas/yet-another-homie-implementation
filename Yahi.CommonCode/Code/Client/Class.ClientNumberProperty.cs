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

                returnValue = Helpers.ParseFloat(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientNumberProperty(ClientPropertyMetadata creationOptions) : base(creationOptions) {
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            bool isPayloadValid;

            isPayloadValid = Helpers.TryParseFloat(payloadToValidate, out _);

            return isPayloadValid;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = Helpers.FloatToString(valueToSet, Format);
                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
