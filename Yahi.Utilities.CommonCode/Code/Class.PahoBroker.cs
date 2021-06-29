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

        public bool TryConnect(string lwtTopic, string lwtPayload) {
            var isOk = true;

            try {
                if ((lwtTopic != "") && (lwtPayload != "")) {
#warning need to figure out what some of those arguments do. It is not clear now. Like, cleanSession and keepAlivePeriod. 
                    _realClient.Connect(_mqttClientGuid, "", "", true, 1, true, lwtTopic, lwtPayload, true, 10);
                }
                else {
                    _realClient.Connect(_mqttClientGuid);
                }
            }
            catch (Exception ex) {
                isOk = false;
            }

            return isOk;
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            PublishReceived(this, new PublishReceivedEventArgs(e.Topic, System.Text.Encoding.UTF8.GetString(e.Message)));
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
