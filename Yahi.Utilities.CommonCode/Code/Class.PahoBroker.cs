using System;
using uPLibrary.Networking.M2Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class PahoBroker : IMqttBroker {
        private MqttClient _realClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        public event PublishReceivedDelegate PublishReceived = delegate { };

        public void Initialize(string ipAddress) {
            _realClient = new MqttClient(ipAddress);
            _realClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public bool TryConnect(string willTopic, string willMessage) {
            var isOk = true;

            try {
                if ((willTopic != "") && (willMessage != "")) {
                    // More information of how those connection parameters work:
                    // http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.pdf
                    _realClient.Connect(
                        clientId: _mqttClientGuid,
                        username: "",
                        password: "",
                        willRetain: true, /* Retains Will message, a default Homie behaviour. */
                        willQosLevel: 1, /* 1 == "At least once", a default Homie behaviour. */
                        willFlag: true, /* Tells broker to use Will mechanism. If true, other fields are also required. */
                        willTopic, /* Must be provided if willFlag is set to true. */
                        willMessage, /* Must be provided if willFlag is set to true. */
                        cleanSession: true, /* It is possible to get missed messages if setting session to dirty. Might be useful in the future?.. */
                        keepAlivePeriod: 10  /* Maximum time in seconds a silence between broker and a client may last. After that, broker will disconnect the client. Note that this is not for user traffic; client sends periodic ping messages regardless of user traffic. */);
                }
                else {
                    _realClient.Connect(_mqttClientGuid);
                }
            }
            catch (Exception) {
                isOk = false;
            }

            return isOk;
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

        public PahoBroker() { }
    }
}
