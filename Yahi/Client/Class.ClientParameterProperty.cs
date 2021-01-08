using System.ComponentModel;

namespace DevBot.Homie {
    public class ClientParameterProperty : INotifyPropertyChanged {
        IBroker _broker;
        readonly string _topicPrefix;
        readonly string _propertyId;

        public string Name { get; private set; }
        public DataType DataType { get; private set; }
        public string Format { get; private set; }
        public string Unit { get; private set; }
        public string Value { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ClientParameterProperty(string topicPrefix, string propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }

        internal void Initialize(IBroker broker) {
            _broker = broker;

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$name", (topic, value) => {
                Name = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$datatype", (topic, value) => {
                DataType = DataType.String;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DataType)));
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$format", (topic, value) => {
                Format = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$unit", (topic, value) => {
                Unit = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Unit)));
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}", (topic, value) => {
                Value = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
            });
        }

        public void SetValue(string valueToSet) {
            _broker.Publish($"{_topicPrefix}/{_propertyId}/set", valueToSet);
        }
    }
}
