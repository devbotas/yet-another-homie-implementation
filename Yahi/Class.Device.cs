using System;
using System.Collections.Generic;

namespace DevBot9.Protocols.Homie {
    public class Device {
        protected string _baseTopic = "temp";
        protected string _deviceId = "some-device";

        protected List<ClientStateProperty> _stateProperties = new List<ClientStateProperty>();
        protected List<ClientCommandProperty> _commandProperties = new List<ClientCommandProperty>();
        protected List<ClientParameterProperty> _parameterProperties = new List<ClientParameterProperty>();

        protected PublishToTopicDelegate _publishToTopicDelegate;
        protected SubscribeToTopicDelegate _subscribeToTopicDelegate;
        protected Dictionary<string, List<Action<string>>> _topicHandlerMap = new Dictionary<string, List<Action<string>>>();

        public delegate void PublishToTopicDelegate(string topic, string payload);
        public delegate void SubscribeToTopicDelegate(string topic);

        public string HomieVersion { get; protected set; } = "4.0.0";
        public string Name { get; protected set; }
        public string State { get; protected set; }

        protected void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            _publishToTopicDelegate = publishToTopicDelegate;
            _subscribeToTopicDelegate = subscribeToTopicDelegate;

            foreach (var property in _stateProperties) {
                property.Initialize(this);
            }
            foreach (var property in _commandProperties) {
                property.Initialize(this);
            }
            foreach (var property in _parameterProperties) {
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

        internal void InternalPropertyPublish(string topic, string value) {
            _publishToTopicDelegate(topic, value);
        }

        internal void InternalPropertySubsribe(string propertyTopic, Action<string> actionToTakeOnReceivedMessage) {
            var fullTopic = $"{_baseTopic}/{_deviceId}/{propertyTopic}";

            if (_topicHandlerMap.ContainsKey(fullTopic) == false) {
                _topicHandlerMap.Add(fullTopic, new List<Action<string>>());
            }

            _topicHandlerMap[fullTopic].Add(actionToTakeOnReceivedMessage);

            _subscribeToTopicDelegate(fullTopic);
        }

    }
}
