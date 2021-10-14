#if NANOFRAMEWORK_1_0

using System;
using System.ComponentModel;
using System.Diagnostics;
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
        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        public void Initialize(string ipAddress, AddToLogDelegate loggingFunction = null) {
            if (string.IsNullOrEmpty(ipAddress)) { throw new ArgumentException("Please provide a correct IP address/hostname.", ipAddress); }
            if (loggingFunction != null) { _log = loggingFunction; }

            _isIpSet = true;

            _realClient = new MqttClient(ipAddress);
            _realClient.MqttMsgPublishReceived += HandlePublishReceived;

            _realClient.ConnectionClosed += (sender, e) => {
                IsConnected = false;
                Disconnected(this, EventArgs.Empty);
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

        public PahoClientDeviceConnection() { }

        private void MonitorMqttConnectionContinuously() {
            var isReadyForConnection = false;

            while (true) {
                isReadyForConnection = (IsConnected == false) && _isIpSet;
                if (_isHostDeviceConnector) { isReadyForConnection &= _isWillSet; }

                if (isReadyForConnection) {
                    try {
                        if (_isHostDeviceConnector) {
                            LogInfo($"Connecting to broker with Last Will \"{_willTopic}:{_willMessage}\".");

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

                        }
                        else {
                            LogInfo($"Connecting to broker without Last Will topic.");
                            _realClient.Connect(_mqttClientGuid);
                        }

                        Thread.Sleep(100);

                        IsConnected = true;
                        Connected(this, EventArgs.Empty);
                    }
                    catch (Exception ex) {
                        Debug.WriteLine($"Problem in {nameof(MonitorMqttConnectionContinuously)}: {ex.Message}");
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

        public void Connect() {

        }

        public void Publish(string topic, string payload, byte qosLevel, bool isRetained) {
            _realClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(payload), qosLevel, isRetained);
#warning This is a patch for the memore leak bug https://github.com/nanoframework/Home/issues/816 . If the issue gets resolved, need to remove it.
            System.Threading.Thread.Sleep(50);
        }

        public void Subscribe(string topic) {
            _realClient.Subscribe(new[] { topic }, new byte[] { 1 });
        }
    }
}
#endif
