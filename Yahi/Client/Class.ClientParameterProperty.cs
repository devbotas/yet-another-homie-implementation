namespace DevBot.Homie {
    public class ClientParameterProperty : ClientPropertyBase {
        public ClientParameterProperty(string topicPrefix, string propertyId) : base(topicPrefix, propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);
        }

        public void SetValue(string valueToSet) {
            _broker.Publish($"{_topicPrefix}/{_propertyId}/set", valueToSet);
        }
    }
}
