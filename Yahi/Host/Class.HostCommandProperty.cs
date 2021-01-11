namespace DevBot.Homie {
    public class HostCommandProperty : HostPropertyBase {
        internal HostCommandProperty(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(topicPrefix, propertyId, friendlyName, dataType, format, true, false, unit) {
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}", (topic, value) => {
                Value = value;
            });
        }
    }
}
