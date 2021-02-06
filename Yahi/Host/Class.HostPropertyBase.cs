using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class HostPropertyBase : PropertyBase {
        protected string _rawValue = "";
        protected readonly string _nameAttribute;
        protected readonly DataType _dataTypeAttribute;
        protected readonly string _formatAttribute;
        protected readonly bool _isSettableAttribute;
        protected readonly bool _isRetainedAttribute;
        protected readonly string _unitAttribute;
        public PropertyType Type { get; protected set; } = PropertyType.State;

        protected HostPropertyBase(PropertyType propertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) {
            Type = propertyType;

            _propertyId = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _formatAttribute = format;
            _unitAttribute = unit;

            switch (Type) {

                case PropertyType.State:
                    _isRetainedAttribute = true;
                    _isSettableAttribute = false;
                    break;

                case PropertyType.Parameter:
                    _isRetainedAttribute = true;
                    _isSettableAttribute = true;
                    break;

                case PropertyType.Command:
                    _isRetainedAttribute = false;
                    _isSettableAttribute = true;
                    break;
            }
        }

        internal override void Initialize(Device parentDevice) {
            _parentDevice = parentDevice;

            _parentDevice.InternalPropertyPublish($"{_propertyId}/$name", _nameAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$datatype", _dataTypeAttribute.ToString().ToLower());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$format", _formatAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$settable", _isSettableAttribute.ToString().ToLower());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$retained", _isRetainedAttribute.ToString().ToLower());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$unit", _unitAttribute);

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));

                        _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    }
                });
            }

            if (Type == PropertyType.Command) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }
        }

        protected virtual bool ValidatePayload(string payloadToValidate) {
            return false;
        }
    }
}
