namespace DevBot.Homie {
    public class ClientCommandProperty : ClientPropertyBase {
        public ClientCommandProperty(string topicPrefix, string propertyId) : base(topicPrefix, propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);
        }

        public void SetValue(string valueToSet) {
            _broker.Publish($"{_topicPrefix}/{_propertyId}", valueToSet);
        }
    }
}
