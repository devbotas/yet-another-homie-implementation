namespace DevBot.Homie {
    public class ClientStateProperty : ClientPropertyBase {
        internal ClientStateProperty(string topicPrefix, string propertyId) : base(topicPrefix, propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }

        internal new void Initialize(IBroker broker) {
            base.Initialize(broker);
        }
    }
}
