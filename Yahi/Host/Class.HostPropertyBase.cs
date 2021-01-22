using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class HostPropertyBase : PropertyBase {
        protected readonly string _nameAttribute;
        protected readonly DataType _dataTypeAttribute;
        protected readonly string _formatAttribute;
        protected readonly bool _isSettableAttribute;
        protected readonly bool _isRetainedAttribute;
        protected readonly string _unitAttribute;

        // Value, as a public property, doesn't make much sense for HostStateProperty, 
        // but putting it here kinda makes all the code more compact...
        protected HomieValue _value = new HomieValue();
        public HomieValue Value {
            get { return _value; }
            protected set {
                _value = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        protected HomieNumber _homieNumber = new HomieNumber();
        public HomieNumber HomieNumber {
            get { return _homieNumber; }
            protected set {
                _homieNumber = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(HomieNumber)));
            }
        }

        protected string _homieString = "";
        public string HomieString {
            get { return _homieString; }
            protected set {
                _homieString = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(HomieString)));
            }
        }


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
