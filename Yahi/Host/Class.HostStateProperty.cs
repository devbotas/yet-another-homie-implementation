namespace DevBot9.Protocols.Homie {
    public class HostStateProperty : HostPropertyBase {
        internal HostStateProperty(string topicPrefix, string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(topicPrefix, propertyId, friendlyName, dataType, format, false, true, unit) {
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);
        }

        public void SetValue(string valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends an external command.
            _value = valueToSet;

            _broker.Publish($"{_topicPrefix}/{_propertyId}", Value);
        }
    }
}
