using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class ClientNumberProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public double Value {
            get {
                double returnValue;

                returnValue = Helpers.ParseDouble(_rawValue);

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

            isPayloadValid = Helpers.TryParseDouble(payloadToValidate, out _);

            return isPayloadValid;
        }

        private void SetValue(double valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    var formatString = "";
                    if (Tags.ContainsKey("Precision")) { formatString = "F" + (string)Tags["Precision"]; }

                    _rawValue = Helpers.DoubleToString(valueToSet, formatString);

                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
