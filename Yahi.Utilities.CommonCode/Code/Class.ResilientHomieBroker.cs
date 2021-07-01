using System;
using System.Collections;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class ResilientHomieBroker {
        public delegate void PublishReceivedDelegate(string topic, string payload);
        public event PublishReceivedDelegate PublishReceived = delegate { };

        public delegate void AddToLogDelegate(string severity, string message);

        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(string mqttBrokerIpAddress, string lwtTopic = "", string lwtPayload = "", AddToLogDelegate loggingFunction = null) {
            if (IsInitialized) { return; }

            _lwtTopic = lwtTopic;
            _lwtPayload = lwtPayload;

            if (loggingFunction != null) { _log = loggingFunction; }

            _mqttBrokerIp = mqttBrokerIpAddress;
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.ConnectionClosed += (sender, e) => {
                LogError("M2Mqtt library reports a ConnectionClosed event.");
                IsConnected = false;
            };

            new Thread(() => MonitorMqttConnectionContinuously()).Start();

            IsInitialized = true;
        }
        public void MonitorMqttConnectionContinuously() {
            while (true) {
                if (IsConnected == false) {
                    try {
                        // Setting LWT stuff on connect, if available.
                        if ((_lwtTopic != "") && (_lwtPayload != "")) {
                            // More information of how those connection parameters work:
                            // http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.pdf
                            LogInfo($"Connecting to broker with Last Will \"{_lwtTopic}:{_lwtPayload}\".");
                            _mqttClient.Connect(_mqttClientGuid, "", "", true, 1, true, _lwtTopic, _lwtPayload, true, 10);
                        }
                        else {
                            LogInfo($"Connecting to broker without Last Will topic.");
                            _mqttClient.Connect(_mqttClientGuid);
                        }

                        // All subscribtions were dropped during disconnect event. Resubscribing.
                        LogInfo($"(Re)subscribing to {_subscriptionList.Count} topic(s).");
                        foreach (string topic in _subscriptionList) {
                            _mqttClient.Subscribe(new[] { topic }, new byte[] { 1 });
                        }

                        IsConnected = true;
                    }
                    catch (Exception ex) {
                        LogError($"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work. Exception: {ex.Message}.");
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            if (IsConnected == false) { return; }

            var retryCount = 0;
            var isPublishSuccessful = false;
            while ((retryCount < 3) && (isPublishSuccessful == false)) {
                try {
                    _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(payload), qosLevel, isRetained);
                    isPublishSuccessful = true;
                }
                catch (Exception ex) {
                    retryCount++;
                    LogError($"Could not publish topic {topic} to broker {_mqttBrokerIp}, attempt {retryCount}. Exception: {ex.Message}.");
                }

            }

            if (isPublishSuccessful == false) {
                LogError($"Too many fails at publishing, going to disconnected state.");
                IsConnected = false;
            }
        }
        public void SubscribeToTopic(string topic) {
            _mqttClient.Subscribe(new string[] { topic }, new byte[] { 2 });
            _subscriptionList.Add(topic);
        }

        // private Logger _log = null;
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "localhost";
        private string _mqttClientGuid = Guid.NewGuid().ToString();
        private string _lwtTopic = "";
        private string _lwtPayload = "";
        private ArrayList _subscriptionList = new ArrayList();
        private AddToLogDelegate _log = delegate { };

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            PublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
        }

        private void LogInfo(string message) {
            _log("Info", message);
        }
        private void LogError(string message) {
            _log("Error", message);
        }
    }
}
