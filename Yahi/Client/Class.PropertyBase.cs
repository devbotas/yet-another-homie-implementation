using System.ComponentModel;

namespace DevBot.Homie {
    public class PropertyBase {
        IBroker _broker;
        readonly string _topicPrefix;
        readonly string _propertyId;

        public string Name { get; private set; }
        public DataType DataType { get; private set; }
        public string Format { get; private set; }
        public string Unit { get; private set; }
        // public string Value { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public PropertyBase(string topicPrefix, string propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }
    }
}
