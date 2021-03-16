using System;
using System.Globalization;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class ClientFloatProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
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

        internal ClientFloatProperty(PropertyType propertyType, string propertyId) : base(propertyId) {
            _rawValue = "0.0";
            Type = propertyType;
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
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = valueToSet.ToString(CultureInfo.InvariantCulture);
                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
