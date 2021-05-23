using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Boolean, as defined by the Homie convention.
    /// </summary>
    public class ClientColorProperty : ClientPropertyBase {
        private ColorFormat _colorFormat = ColorFormat.Rgb;

        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public HomieColor Value {
            get {
                HomieColor returnValue;

                if (_colorFormat == ColorFormat.Rgb) { returnValue = HomieColor.FromRgbString(_rawValue); }
                else { returnValue = HomieColor.FromHsvString(_rawValue); }

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientColorProperty(ClientPropertyMetadata creationProperties) : base(creationProperties) {
            if (Helpers.TryParseBool(_rawValue, out var _) == false) { _rawValue = "0,0,0"; }

            // This will always pass, because incorrect Format string will throw an exception in constructors above.
            var alwaysTrue = Helpers.TryParseHomieColorFormat(Format, out _colorFormat);
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var areNumbersGood = HomieColor.ValidatePayload(payloadToValidate, _colorFormat);

            return areNumbersGood;
        }

        private void SetValue(HomieColor valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    if (_colorFormat == ColorFormat.Rgb) { _rawValue = valueToSet.ToRgbString(); }
                    else { _rawValue = valueToSet.ToHsvString(); }

                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
