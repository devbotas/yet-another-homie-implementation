using System;
using System.Collections.Generic;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a base class for the Host and Client Device class implementation. One should never use it directly (not really even possible since the constructor is not public).
    /// </summary>
    public class Device {
        #region Public interface

        /// <summary>
        /// This delegate is used by YAHI to publish messages to the external MQTT broker.
        /// </summary>
        public delegate void PublishToTopicDelegate(string topic, string payload, byte qosLevel, bool isRetained);

        /// <summary>
        /// This delegate is used by YAHI to subscribe to MQTT topics.
        /// </summary>
        /// <param name="topic"></param>
        public delegate void SubscribeToTopicDelegate(string topic);

        /// <summary>
        /// Homie convention version that YAHI complies to.
        /// </summary>
        public string HomieVersion { get; protected set; } = "4.0.0";

        /// <summary>
        /// Name of the device. This becomes a parent topic for all the nodes and properties.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// State of the device. Homie convention defines these: init, ready, disconnected, sleeping, lost, alert.
        /// </summary>
        public HomieState State { get; protected set; }

        /// <summary>
        /// Call this function to pass incoming publish events from the external MQTT broker. YAHI will then handle the payload and raise events, if needed.
        /// </summary>
        public void HandlePublishReceived(string fullTopic, string payload) {
            if (_topicHandlerMap.ContainsKey(fullTopic)) {
                foreach (var handler in _topicHandlerMap[fullTopic]) {
                    handler(payload);
                }
            }
        }

        /// <summary>
        /// Returns an array of all topics that this device instance has published to. This is more for debugging. Function may be removed in 1.x release.
        /// </summary>
        public string[] GetAllPublishedTopics() {
            var returnArray = _publishedTopics.ToArray();

            return returnArray;
        }

        #endregion

        #region Private stuff
        protected string _baseTopic = "temp";
        protected string _deviceId = "some-device";

        protected List<PropertyBase> _properties = new List<PropertyBase>();

        protected PublishToTopicDelegate _publishToTopicDelegate;
        protected SubscribeToTopicDelegate _subscribeToTopicDelegate;
        protected Dictionary<string, List<Action<string>>> _topicHandlerMap = new Dictionary<string, List<Action<string>>>();

        protected List<string> _publishedTopics = new List<string>();

        protected Device() {
            // Just making public constructor unavailable to user, as this class should not be consumed directly.
        }

        protected void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            _publishToTopicDelegate = publishToTopicDelegate;
            _subscribeToTopicDelegate = subscribeToTopicDelegate;

            foreach (var property in _properties) {
                property.Initialize(this);
            }
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
            _publishToTopicDelegate(topicId, value, 1, true);
        }
        #endregion
    }
}
