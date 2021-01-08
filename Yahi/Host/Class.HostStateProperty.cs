namespace DevBot.Homie {
    public class HostStateProperty {
        readonly string _nameAttribute = "";
        readonly DataType _dataTypeAttribute = DataType.String;
        readonly string _formatAttribute = "";
        readonly bool _isSettableAttribute = false;
        readonly bool _isRetainedAttribute = true;
        readonly string _unitAttribute;
        readonly string _nameAttributeTopic;
        readonly string _dataTypeAttributeTopic;
        readonly string _formatAttributeAttributeTopic;
        readonly string _isSettableAttributeTopic;
        readonly string _isRetainedAttributeTopic;
        readonly string _unitAttributeTopic;
        readonly string _valueTopic;

        string _value = "";
        readonly string _id;
        readonly string _topicPrefix;

        IBroker _broker;

        internal HostStateProperty(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string unit) {
            _topicPrefix = topicPrefix;
            _id = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _unitAttribute = unit;

            _nameAttributeTopic = $"{_topicPrefix}/{_id}/$name";
            _dataTypeAttributeTopic = $"{_topicPrefix}/{_id}/$datatype";
            _formatAttributeAttributeTopic = $"{_topicPrefix}/{_id}/$format";
            _isSettableAttributeTopic = $"{_topicPrefix}/{_id}/$settable";
            _isRetainedAttributeTopic = $"{_topicPrefix}/{_id}/$retained";
            _unitAttributeTopic = $"{_topicPrefix}/{_id}/$unit";

            _valueTopic = $"{_topicPrefix}/{_id}";
        }

        internal void Initialize(IBroker broker) {
            _broker = broker;

            _broker.Publish(_nameAttributeTopic, _nameAttribute);
            _broker.Publish(_dataTypeAttributeTopic, _dataTypeAttribute.ToString());
            _broker.Publish(_formatAttributeAttributeTopic, _formatAttribute);
            _broker.Publish(_isSettableAttributeTopic, _isSettableAttribute.ToString());
            _broker.Publish(_isRetainedAttributeTopic, _isRetainedAttribute.ToString());
            _broker.Publish(_unitAttributeTopic, _unitAttribute);
        }

        public void SetValue(string valueToSet) {
            _value = valueToSet;
            _broker.Publish(_valueTopic, _value);
        }
    }
}
