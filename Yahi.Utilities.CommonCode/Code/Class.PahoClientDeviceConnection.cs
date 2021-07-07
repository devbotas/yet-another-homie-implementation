using System;
using System.ComponentModel;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class PahoClientDeviceConnection : IClientDeviceConnection {
        protected bool _isHostDeviceConnector;
        protected MqttClient _realClient;
        protected string _mqttClientGuid = Guid.NewGuid().ToString();
        protected string _willTopic = "";
        protected string _willMessage = "";

        protected bool _isWillSet;
        protected bool _isIpSet;

        protected AddToLogDelegate _log = delegate { };

        public bool IsConnected { get; private set; }

        public event PublishReceivedDelegate PublishReceived = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void Initialize(string ipAddress, AddToLogDelegate loggingFunction = null) {
            if (string.IsNullOrEmpty(ipAddress)) { throw new ArgumentException("Please provide a correct IP address/hostname.", ipAddress); }
            if (loggingFunction != null) { _log = loggingFunction; }

            _isIpSet = true;

            _realClient = new MqttClient(ipAddress);
            _realClient.MqttMsgPublishReceived += HandlePublishReceived;

            _realClient.ConnectionClosed += (sender, e) => {
                IsConnected = false;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            };

            // Spinning up connection monitor.
            new Thread(() => MonitorMqttConnectionContinuously()).Start();

            // Giving some time for the connection to happen.It is important, because property initializers will start subscribe immediatelly.
            for (var i = 0; i < 10; i++) {
                Thread.Sleep(100);

                if (IsConnected) { break; }
            }
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            PublishReceived(this, new PublishReceivedEventArgs(e.Topic, System.Text.Encoding.UTF8.GetString(e.Message, 0, e.Message.Length)));
        }

        public void Disconnect() {
            _realClient.Disconnect();
        }

        public bool TryPublish(string topic, string payload, byte qosLevel, bool isRetained) {
            var isOk = true;

            if (_realClient.IsConnected) {
                _realClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(payload), qosLevel, isRetained);
            }
            else {
                isOk = false;
            }

            return isOk;
        }

        public bool TrySubscribe(string topic) {
            var isOk = true;

            if (_realClient.IsConnected) {
                _realClient.Subscribe(new[] { topic }, new byte[] { 1 });
            }
            else {
                isOk = false;
            }

            return isOk;
        }

        public PahoClientDeviceConnection() { }

        private void MonitorMqttConnectionContinuously() {
            while (true) {
                if ((IsConnected == false) && _isIpSet) {
                    if (_isHostDeviceConnector && _isWillSet) {
                        LogInfo($"Connecting to broker with Last Will \"{_willTopic}:{_willMessage}\".");
                        try {
                            // More information of how those connection parameters work:
                            // http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.pdf
                            _realClient.Connect(
                                clientId: _mqttClientGuid,
                                username: "",
                                password: "",
                                willRetain: true, /* Retains Will message, a default Homie behaviour. */
                                willQosLevel: 1, /* 1 == "At least once", a default Homie behaviour. */
                                willFlag: true, /* Tells broker to use Will mechanism. If true, other fields are also required. */
                                _willTopic, /* Must be provided if willFlag is set to true. */
                                _willMessage, /* Must be provided if willFlag is set to true. */
                                cleanSession: true, /* It is possible to get missed messages if setting session to dirty. Might be useful in the future?.. */
                                keepAlivePeriod: 10  /* Maximum time in seconds a silence between broker and a client may last. After that, broker will disconnect the client. Note that this is not for user traffic; client sends periodic ping messages regardless of user traffic. */);

                            IsConnected = true;
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                        }
                        catch (Exception) {
                            // Swallowing.
                        }
                    }

                    if (_isHostDeviceConnector == false) {
                        LogInfo($"Connecting to broker without Last Will topic.");
                        try {
                            _realClient.Connect(_mqttClientGuid);

                            Thread.Sleep(100);

                            IsConnected = true;
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                        }
                        catch (Exception) {
                            // Swallowing.
                        }
                    }

                    if (IsConnected == false) { LogError($"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work."); }

                }

                Thread.Sleep(1000);
            }
        }
        protected void LogInfo(string message) {
            _log("Info", message);
        }

        protected void LogError(string message) {
            _log("Error", message);
        }
    }
}
