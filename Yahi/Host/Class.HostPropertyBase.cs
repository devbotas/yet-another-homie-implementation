namespace DevBot9.Protocols.Homie {
    public class HostPropertyBase : PropertyBase {
        protected string _rawValue = "";
        protected readonly string _nameAttribute;
        protected readonly DataType _dataTypeAttribute;
        protected readonly string _formatAttribute;
        protected readonly bool _isSettableAttribute;
        protected readonly bool _isRetainedAttribute;
        protected readonly string _unitAttribute;

        protected HostPropertyBase(string propertyId, string friendlyName, DataType dataType, string format, bool isSettable, bool isRetained, string unit) {
            _propertyId = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _formatAttribute = format;
            _isSettableAttribute = isSettable;
            _isRetainedAttribute = isRetained;
            _unitAttribute = unit;
        }

        internal override void Initialize(Device parentDevice) {
            _parentDevice = parentDevice;

            _parentDevice.InternalPropertyPublish($"{_propertyId}/$name", _nameAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$datatype", _dataTypeAttribute.ToString());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$format", _formatAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$settable", _isSettableAttribute.ToString());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$retained", _isRetainedAttribute.ToString());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$unit", _unitAttribute);
        }
    }
}
