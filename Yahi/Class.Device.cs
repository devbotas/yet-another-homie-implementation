using System;
using System.Collections.Generic;

namespace DevBot9.Protocols.Homie {
    public class Device {
        protected string _baseTopic = "temp";
        protected string _deviceId = "some-device";

        protected List<PropertyBase> _properties = new List<PropertyBase>();

        protected PublishToTopicDelegate _publishToTopicDelegate;
        protected SubscribeToTopicDelegate _subscribeToTopicDelegate;
        protected Dictionary<string, List<Action<string>>> _topicHandlerMap = new Dictionary<string, List<Action<string>>>();

        protected List<string> _publishedTopics = new List<string>();

        public delegate void PublishToTopicDelegate(string topic, string payload);
        public delegate void SubscribeToTopicDelegate(string topic);

        public string HomieVersion { get; protected set; } = "3.0.0";
        public string Name { get; protected set; }
        public string State { get; protected set; }

        protected void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            _publishToTopicDelegate = publishToTopicDelegate;
            _subscribeToTopicDelegate = subscribeToTopicDelegate;

            foreach (var property in _properties) {
                property.Initialize(this);
            }

            //_broker.Publish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            //_broker.Publish($"{_baseTopic}/{_deviceId}/$name", Name);
            //_client.Publish($"homie/{_deviceId}/$nodes", GetNodesString());
            //_client.Publish($"homie/{_deviceId}/$extensions", GetExtensionsString());
        }

        public void HandlePublishReceived(string fullTopic, string payload) {
            if (_topicHandlerMap.ContainsKey(fullTopic)) {
                foreach (var handler in _topicHandlerMap[fullTopic]) {
                    handler(payload);
                }
            }
        }

        public string[] GetAllPublishedTopics() {
            var returnArray = _publishedTopics.ToArray();

            return returnArray;
        }

        internal void InternalPropertyPublish(string propertyTopic, string value) {
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{propertyTopic}", value);
        }

        internal void InternalPropertySubscribe(string propertyTopic, Action<string> actionToTakeOnReceivedMessage) {
            var fullTopic = $"{_baseTopic}/{_deviceId}/{propertyTopic}";

            if (_topicHandlerMap.ContainsKey(fullTopic) == false) {
                _topicHandlerMap.Add(fullTopic, new List<Action<string>>());
            }

            _topicHandlerMap[fullTopic].Add(actionToTakeOnReceivedMessage);

            _subscribeToTopicDelegate(fullTopic);
        }

        internal void InternalGeneralPublish(string topicId, string value) {
            if (_publishedTopics.Contains(topicId) == false) {
                _publishedTopics.Add(topicId);
            }
            _publishToTopicDelegate(topicId, value);
        }
    }
}
