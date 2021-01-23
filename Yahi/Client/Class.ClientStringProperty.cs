using System;

namespace DevBot9.Protocols.Homie {
    public class ClientStringProperty : ClientPropertyBase {
        public string Value {
            get {
                return _rawValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientStringProperty(PropertyType propertyType, string propertyId) : base(propertyId) {
            Type = propertyType;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);

        }

        protected override bool ValidatePayload(string payloadToValidate) {
            return true;
        }

        private void SetValue(string valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = valueToSet;
                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
