using System.ComponentModel;

namespace DevBot.Homie {
    public class HostCommandProperty : INotifyPropertyChanged {
        readonly string _nameAttribute = "";
        readonly DataType _dataTypeAttribute = DataType.String;
        readonly string _formatAttribute = "";
        readonly bool _isSettableAttribute = true;
        readonly bool _isRetainedAttribute = false;
        readonly string _unitAttribute;
        readonly string _nameAttributeTopic;
        readonly string _dataTypeAttributeTopic;
        readonly string _formatAttributeAttributeTopic;
        readonly string _isSettableAttributeTopic;
        readonly string _isRetainedAttributeTopic;
        readonly string _unitAttributeTopic;
        readonly string _valueTopic;

        public string Value { get; private set; }

        readonly string _propertyId;
        readonly string _topicPrefix;

        IBroker _broker;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        internal HostCommandProperty(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string unit) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _unitAttribute = unit;

            _nameAttributeTopic = $"{_topicPrefix}/{_propertyId}/$name";
            _dataTypeAttributeTopic = $"{_topicPrefix}/{_propertyId}/$datatype";
            _formatAttributeAttributeTopic = $"{_topicPrefix}/{_propertyId}/$format";
            _isSettableAttributeTopic = $"{_topicPrefix}/{_propertyId}/$settable";
            _isRetainedAttributeTopic = $"{_topicPrefix}/{_propertyId}/$retained";
            _unitAttributeTopic = $"{_topicPrefix}/{_propertyId}/$unit";

            _valueTopic = $"{_topicPrefix}/{_propertyId}";
        }

        internal void Initialize(IBroker broker) {
            _broker = broker;

            _broker.Subscribe(_valueTopic, (topic, value) => {
                Value = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
            });

            _broker.Publish(_nameAttributeTopic, _nameAttribute);
            _broker.Publish(_dataTypeAttributeTopic, _dataTypeAttribute.ToString());
            _broker.Publish(_formatAttributeAttributeTopic, _formatAttribute);
            _broker.Publish(_isSettableAttributeTopic, _isSettableAttribute.ToString());
            _broker.Publish(_isRetainedAttributeTopic, _isRetainedAttribute.ToString());
            _broker.Publish(_unitAttributeTopic, _unitAttribute);
        }
    }
}
