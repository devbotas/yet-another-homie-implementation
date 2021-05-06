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

                if (_colorFormat == ColorFormat.Rgb) {
                    returnValue = new HomieColor();
                    returnValue.SetRgb(_rawValue);
                }
                else {
                    returnValue = new HomieColor();
                    returnValue.SetHsv(_rawValue);
                }

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientColorProperty(ClientPropertyMetadata creationProperties) : base(creationProperties) {
            if (Helpers.TryParseBool(_rawValue, out var _) == false) { _rawValue = "false"; }

            Helpers.TryParseHomieColorFormat(Format, out _colorFormat);
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var colorParts = payloadToValidate.Split(',');
            if (colorParts.Length != 3) { return false; }

            var areNumbersGood = true;
            if (Format == ColorFormat.Rgb.ToHomiePayload()) {
                if (Helpers.TryParseInt(colorParts[0], out var red)) {
                    if (red < 0) { areNumbersGood &= false; }
                    if (red > 255) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[1], out var green)) {
                    if (green < 0) { areNumbersGood &= false; }
                    if (green > 255) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[2], out var blue)) {
                    if (blue < 0) { areNumbersGood &= false; }
                    if (blue > 255) { areNumbersGood &= false; }
                }
            }
            if (Format == ColorFormat.Hsv.ToHomiePayload()) {
                if (Helpers.TryParseInt(colorParts[0], out var hue)) {
                    if (hue < 0) { areNumbersGood &= false; }
                    if (hue > 360) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[1], out var saturation)) {
                    if (saturation < 0) { areNumbersGood &= false; }
                    if (saturation > 100) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[2], out var value)) {
                    if (value < 0) { areNumbersGood &= false; }
                    if (value > 100) { areNumbersGood &= false; }
                }
            }

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
