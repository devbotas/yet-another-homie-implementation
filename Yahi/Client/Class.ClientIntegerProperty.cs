﻿using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Integer, as defined by the Homie convention.
    /// </summary>
    public class ClientIntegerProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public int Value {
            get {
                int returnValue;
                returnValue = int.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientIntegerProperty(PropertyType propertyType, string propertyId) : base(propertyId) {
            _rawValue = "0";
            Type = propertyType;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = int.TryParse(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(int valueToSet) {
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
