using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    public class HomieTopicFetcher {
        private MqttClient _mqttClient;
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();
        private List<string> _responses = new();

        public void Initialize(string mqttBrokerIpAddress) {
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void FetchTopics(string filter, out string[] topics) {
            _responses.Clear();
            _mqttClient.Subscribe(new[] { filter }, new byte[] { 1 });
            _mqttClient.Connect(_mqttClientGuid);
            Thread.Sleep(2000);
            _mqttClient.Unsubscribe(new[] { filter });
            _mqttClient.Disconnect();

            topics = _responses.ToArray();
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            var payload = Encoding.UTF8.GetString(e.Message);
            _responses.Add(e.Topic + ":" + payload);
        }
    }
}
