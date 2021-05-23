using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using uPLibrary.Networking.M2Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class ResilientHomieBroker {
        public delegate void PublishReceivedDelegate(string topic, string payload);
        public event PublishReceivedDelegate PublishReceived = delegate { };

        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(string mqttBrokerIpAddress, string lwtTopic = "", string lwtPayload = "", Logger logger = null) {
            if (IsInitialized) { return; }

            _lwtTopic = lwtTopic;
            _lwtPayload = lwtPayload;

            if (logger != null) { _log = logger; }
            else { _log = LogManager.GetCurrentClassLogger(); }

            _globalCancellationTokenSource = new CancellationTokenSource();

            _mqttBrokerIp = mqttBrokerIpAddress;
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.ConnectionClosed += (sender, e) => {
                _log.Error("M2Mqtt library reports a ConnectionClosed event.");
                IsConnected = false;
            };

            Task.Run(async () => await MonitorMqttConnectionContinuously(_globalCancellationTokenSource.Token));

            IsInitialized = true;
        }
        public async Task MonitorMqttConnectionContinuously(CancellationToken cancelationToken) {
            while (cancelationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        // Setting LWT stuff on connect, if available.
                        if ((_lwtTopic != "") && (_lwtPayload != "")) {
#warning need to figure out what some of those arguments do. It is not clear now. Like, cleanSession and keepAlivePeriod. 
                            _log.Info($"Connecting to broker with Last Will \"{_lwtTopic}:{_lwtPayload}\".");
                            _mqttClient.Connect(_mqttClientGuid, "", "", true, 1, true, _lwtTopic, _lwtPayload, true, 10);
                        }
                        else {
                            _log.Info($"Connecting to broker without Last Will topic.");
                            _mqttClient.Connect(_mqttClientGuid);
                        }

                        // All subscribtions were dropped during disconnect event. Resubscribing.
                        _log.Info($"(Re)subscribing to {_subscriptionList.Count} topic(s).");
                        foreach (string topic in _subscriptionList) {
                            _mqttClient.Subscribe(new[] { topic }, new byte[] { 1 });
                        }

                        IsConnected = true;
                    }
                    catch (Exception ex) {
                        _log.Error(ex, $"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }
                }
                await Task.Delay(1000, cancelationToken);
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
                    _log.Error(ex, $"Could not publish topic {topic} to broker {_mqttBrokerIp}, attempt {retryCount}");
                }

            }

            if (isPublishSuccessful == false) {
                _log.Error($"Too many fails at publishing, going to disconnected state.");
                IsConnected = false;
            }
        }
        public void SubscribeToTopic(string topic) {
            _mqttClient.Subscribe(new string[] { topic }, new byte[] { 2 });
            _subscriptionList.Add(topic);
        }

        private CancellationTokenSource _globalCancellationTokenSource;
        private Logger _log = null;
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "localhost";
        private string _mqttClientGuid = Guid.NewGuid().ToString();
        private string _lwtTopic = "";
        private string _lwtPayload = "";
        private ArrayList _subscriptionList = new ArrayList();

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            PublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
