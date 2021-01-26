using System;

namespace DevBot9.Protocols.Homie {
    public class ClientIntegerProperty : ClientPropertyBase {
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

        internal ClientIntegerProperty(PropertyType protertyType, string propertyId) : base(propertyId) {
            _rawValue = "0";
            Type = protertyType;
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
