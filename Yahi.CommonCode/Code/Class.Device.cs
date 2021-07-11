using System;
using System.Collections;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a base class for the Host and Client Device class implementation. One should never use it directly (not really even possible since the constructor is not public).
    /// </summary>
    public class Device : INotifyPropertyChanged, IDisposable {
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
        public bool IsConnected { get { return _broker.IsConnected; } }

        /// <summary>
        /// Is raised when any of the Device properties (HomieVersion, Name, State) changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region Internal Homie guts 

        protected AddToLogDelegate _log = delegate { };
        protected string _baseTopic = "no-base-topic";
        protected ArrayList _properties = new ArrayList();
        protected Hashtable _topicHandlerMap = new Hashtable();
        protected IBasicDeviceConnection _broker;
        private ArrayList _subscriptionList = new ArrayList();

        protected Device() {
            // Just making public constructor unavailable to user, as this class should not be consumed directly.
        }

        protected void Initialize(IBasicDeviceConnection broker, AddToLogDelegate loggingFunction = null) {
            _broker = broker;
            _broker.PublishReceived += HandleBrokerPublishReceived;

            _broker.PropertyChanged += HandleBrokerPropertyChanged;

            if (loggingFunction != null) { _log = loggingFunction; }
        }

        public void Dispose() {
            if (_broker == null) { return; }

            _broker.PublishReceived -= HandleBrokerPublishReceived;
            _broker.PropertyChanged -= HandleBrokerPropertyChanged;
        }

        private void HandleBrokerPublishReceived(object sender, PublishReceivedEventArgs e) {
            if (_topicHandlerMap.Contains(e.Topic)) {
                var zeList = (ArrayList)_topicHandlerMap[e.Topic];
                foreach (ActionStringDelegate handler in zeList) {
                    handler(e.Payload);
                }
            }
        }

        private void HandleBrokerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if ((e.PropertyName == nameof(_broker.IsConnected)) && _broker.IsConnected) {
                // All subscribtions were dropped during disconnect event. Resubscribing.
                var clonedSubsribtionTable = (ArrayList)_subscriptionList.Clone();
                LogInfo($"(Re)subscribing to {clonedSubsribtionTable.Count} topic(s).");
                foreach (string topic in clonedSubsribtionTable) {
                    _broker.TrySubscribe(topic);
                }
            }
        }


        internal virtual void InternalPropertyPublish(string propertyTopic, string value, bool isRetained = true) {
            InternalGeneralPublish($"{_baseTopic}/{DeviceId}/{propertyTopic}", value, isRetained);
        }

        internal void InternalPropertySubscribe(string propertyTopic, ActionStringDelegate actionToTakeOnReceivedMessage) {
            InternalGeneralSubscribe($"{_baseTopic}/{DeviceId}/{propertyTopic}", actionToTakeOnReceivedMessage);
        }

        internal void InternalGeneralPublish(string topicId, string value, bool isRetained = true) {
            if (IsConnected) { _broker.TryPublish(topicId, value, 1, isRetained); }



            //var retryCount = 0;
            //var isPublishSuccessful = false;
            //while ((retryCount < 3) && (isPublishSuccessful == false)) {
            //    if (_broker.TryPublish(topicId, value, 1, true)) {
            //        isPublishSuccessful = true;
            //    }
            //    else {
            //        retryCount++;
            //        LogError($"Could not publish topic {topicId} to broker, attempt {retryCount}.");
            //    }
            //}

            //if (isPublishSuccessful == false) {
            //    LogError($"Too many fails at publishing, going to disconnected state.");
            //    IsConnected = false;
            //}
        }

        internal void InternalGeneralSubscribe(string topicId, ActionStringDelegate actionToTakeOnReceivedMessage) {
            var fullTopic = topicId;

            // Keeping a subscribtion topic list, because it is needed when (re)connecting to broker.
            if (_topicHandlerMap.Contains(fullTopic) == false) {
                _topicHandlerMap.Add(fullTopic, new ArrayList());
            }

            ((ArrayList)_topicHandlerMap[fullTopic]).Add(actionToTakeOnReceivedMessage);

            _broker.TrySubscribe(fullTopic);
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
    }
}
