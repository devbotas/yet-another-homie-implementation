using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class HostNumberProperty : HostPropertyBase {
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

        internal HostNumberProperty(PropertyType propertyType, string propertyId, string friendlyName, double initialValue, int decimalPlaces, string unit) : base(propertyType, propertyId, friendlyName, DataType.Float, "", unit) {
            _tags.Add("Precision", decimalPlaces.ToString());
            _rawValue = Helpers.DoubleToString(initialValue, "F" + decimalPlaces);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = Helpers.TryParseDouble(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(double valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    var formatString = "";
                    if (_tags.Contains("Precision")) { formatString = "F" + (string)_tags["Precision"]; }

                    _rawValue = Helpers.DoubleToString(valueToSet, formatString);

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
