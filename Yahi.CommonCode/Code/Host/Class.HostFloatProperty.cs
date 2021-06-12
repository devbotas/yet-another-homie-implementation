﻿using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class HostFloatProperty : HostPropertyBase {
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

        internal HostFloatProperty(PropertyType propertyType, string propertyId, string friendlyName, float initialValue, int decimalPlaces, string unit) : base(propertyType, propertyId, friendlyName, DataType.Float, Helpers.GetFloatFormatString(decimalPlaces), unit) {
            _rawValue = Helpers.FloatToString(initialValue, _formatAttribute);
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = Helpers.TryParseFloat(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:

                    _rawValue = Helpers.FloatToString(valueToSet, _formatAttribute);

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}