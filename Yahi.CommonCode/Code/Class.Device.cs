using System.Collections;
using System.ComponentModel;
using System.Threading;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a base class for the Host and Client Device class implementation. One should never use it directly (not really even possible since the constructor is not public).
    /// </summary>
    public class Device : INotifyPropertyChanged {
        #region Public interface

        /// <summary>
        /// Homie convention version that YAHI complies to.
        /// </summary>
        public string HomieVersion { get; protected set; } = "4.0.0";

        /// <summary>
        /// Friendly name of the device.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// This becomes a parent topic for all the nodes and properties.
        /// </summary>
        public string DeviceId { get; protected set; }

        /// <summary>
        /// State of the device. Homie convention defines these: init, ready, disconnected, sleeping, lost, alert.
        /// </summary>
        public HomieState State { get; protected set; }

        /// <summary>
        /// Shows if this device is actually connected to broker.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Is raised when any of the Device properties (HomieVersion, Name, State) changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Returns an array of all topics that this device instance has published to. This is more for debugging. Function may be removed in 1.x release.
        /// </summary>
        public string[] GetAllPublishedTopics() {
            var returnArray = (string[])_publishedTopics.ToArray(typeof(string));

            return returnArray;
        }
        #endregion

        #region Internal Homie guts 

        protected AddToLogDelegate _log = delegate { };
        protected string _baseTopic = "no-base-topic";
        protected ArrayList _properties = new ArrayList();
        protected Hashtable _topicHandlerMap = new Hashtable();
        protected ArrayList _publishedTopics = new ArrayList();

        protected Device() {
            // Just making public constructor unavailable to user, as this class should not be consumed directly.
        }

        protected void Initialize(IMqttBroker mqttBroker, AddToLogDelegate loggingFunction = null) {
            _broker = mqttBroker;
            _broker.PublishReceived += (sender, e) => {
                if (_topicHandlerMap.Contains(e.Topic)) {
                    var zeList = (ArrayList)_topicHandlerMap[e.Topic];
                    foreach (ActionString handler in zeList) {
                        handler(e.Payload);
                    }
                }
            };

            if (loggingFunction != null) { _log = loggingFunction; }

            // Spinning up connection monitor.
            new Thread(() => MonitorMqttConnectionContinuously()).Start();

            // Giving some time for the connection to happen. It is important, because property initializers will start subscribe immediatelly.
            for (var i = 0; i < 10; i++) {
                Thread.Sleep(100);

                if (IsConnected) { break; }
            }

            // Initializing properties. Theyy will start using broker immediatelly.
            foreach (PropertyBase property in _properties) {
                property.Initialize(this);
            }
        }

        internal void InternalPropertyPublish(string propertyTopic, string value) {
            InternalGeneralPublish($"{_baseTopic}/{DeviceId}/{propertyTopic}", value);
        }

        internal void InternalPropertySubscribe(string propertyTopic, ActionString actionToTakeOnReceivedMessage) {
            InternalGeneralSubscribe($"{_baseTopic}/{DeviceId}/{propertyTopic}", actionToTakeOnReceivedMessage);
        }

        internal void InternalGeneralPublish(string topicId, string value) {
            if (_publishedTopics.Contains(topicId) == false) {
                _publishedTopics.Add(topicId);
            }

            if (IsConnected == false) { return; }

            var retryCount = 0;
            var isPublishSuccessful = false;
            while ((retryCount < 3) && (isPublishSuccessful == false)) {
                if (_broker.TryPublish(topicId, value, 1, true)) {
                    isPublishSuccessful = true;
                }
                else {
                    retryCount++;
                    LogError($"Could not publish topic {topicId} to broker, attempt {retryCount}.");
                }
            }

            if (isPublishSuccessful == false) {
                LogError($"Too many fails at publishing, going to disconnected state.");
                IsConnected = false;
            }
        }

        internal void InternalGeneralSubscribe(string topicId, ActionString actionToTakeOnReceivedMessage) {
            var fullTopic = topicId;

            if (_topicHandlerMap.Contains(fullTopic) == false) {
                _topicHandlerMap.Add(fullTopic, new ArrayList());
            }

            ((ArrayList)_topicHandlerMap[fullTopic]).Add(actionToTakeOnReceivedMessage);

            _broker.TrySubscribe(fullTopic/*, new byte[] { 2 }*/);
            _subscriptionList.Add(fullTopic);
        }

        internal void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged(sender, e);
        }

        internal void LogInfo(string message) {
            _log("Info", message);
        }

        internal void LogError(string message) {
            _log("Error", message);
        }
        #endregion

        #region Connection related stuff

        private IMqttBroker _broker;
        private ArrayList _subscriptionList = new ArrayList();
        protected string _willTopic = "";
        protected string _willPayload = "";

        private void MonitorMqttConnectionContinuously() {
            while (true) {
                if (IsConnected == false) {
                    // Setting LWT stuff on connect, if available.
                    if ((_willTopic != "") && (_willPayload != "")) {
                        LogInfo($"Connecting to broker with Last Will \"{_willTopic}:{_willPayload}\".");
                    }
                    else {
                        LogInfo($"Connecting to broker without Last Will topic.");
                    }
                    if (_broker.TryConnect(_willTopic, _willPayload)) {
                        // All subscribtions were dropped during disconnect event. Resubscribing.
                        LogInfo($"(Re)subscribing to {_subscriptionList.Count} topic(s).");
                        foreach (string topic in _subscriptionList) {
                            _broker.TrySubscribe(topic/*, new byte[] { 1 }*/);
                        }

                        IsConnected = true;
                    }
                    else {
                        LogError($"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }

                }

                Thread.Sleep(1000);
            }
        }
        #endregion
    }
}
