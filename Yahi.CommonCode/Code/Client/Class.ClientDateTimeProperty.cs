using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type DateTime, as defined by the Homie convention.
    /// </summary>
    public class ClientDateTimeProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public DateTime Value {
            get {
                DateTime returnValue;

                returnValue = Helpers.ParseDateTime(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientDateTimeProperty(ClientPropertyMetadata creationOptions) : base(creationOptions) {

        }

        protected override bool ValidatePayload(string payloadToValidate) {
            bool isPayloadValid;

            isPayloadValid = Helpers.TryParseDateTime(payloadToValidate, out _);

            return isPayloadValid;
        }

        private void SetValue(DateTime valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = valueToSet.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
