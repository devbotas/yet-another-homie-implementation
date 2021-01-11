namespace DevBot.Homie {
    public class HostParameterProperty : HostPropertyBase {
        internal HostParameterProperty(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(topicPrefix, propertyId, friendlyName, dataType, format, true, true, unit) {
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);

            _broker.Subscribe($"{ _topicPrefix}/{ _propertyId}/set", (topic, value) => {
                SetValue(value);
            });
        }
        public void SetValue(string valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends a new parameter value.
            _value = valueToSet;
            _broker.Publish($"{_topicPrefix}/{_propertyId}", Value);
        }
    }
}
