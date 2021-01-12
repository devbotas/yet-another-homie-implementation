using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class HostPropertyBase : INotifyPropertyChanged {
        protected IBroker _broker;

        protected readonly string _propertyId;
        protected readonly string _topicPrefix;
        protected readonly string _nameAttribute;
        protected readonly DataType _dataTypeAttribute;
        protected readonly string _formatAttribute;
        protected readonly bool _isSettableAttribute;
        protected readonly bool _isRetainedAttribute;
        protected readonly string _unitAttribute;

        // Value, as a public property, doesn't make much sense for HostStateProperty, 
        // but putting it here kinda makes all the code more compact...
        protected string _value;
        public string Value {
            get { return _value; }
            protected set {
                _value = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected HostPropertyBase(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string format, bool isSettable, bool isRetained, string unit) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _formatAttribute = format;
            _isSettableAttribute = isSettable;
            _isRetainedAttribute = isRetained;
            _unitAttribute = unit;
        }

        protected void Initialize(IBroker broker) {
            _broker = broker;

            _broker.Publish($"{_topicPrefix}/{_propertyId}/$name", _nameAttribute);
            _broker.Publish($"{_topicPrefix}/{_propertyId}/$datatype", _dataTypeAttribute.ToString());
            _broker.Publish($"{_topicPrefix}/{_propertyId}/$format", _formatAttribute);
            _broker.Publish($"{_topicPrefix}/{_propertyId}/$settable", _isSettableAttribute.ToString());
            _broker.Publish($"{_topicPrefix}/{_propertyId}/$retained", _isRetainedAttribute.ToString());
            _broker.Publish($"{_topicPrefix}/{_propertyId}/$unit", _unitAttribute);
        }
    }
}
