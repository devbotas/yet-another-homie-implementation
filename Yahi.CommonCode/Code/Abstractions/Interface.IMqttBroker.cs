namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Used to wrap around concrete broker implementations (like M2Mqtt or MQTT.net) so that <see cref="Device"/> class could consume it.
    /// </summary>
    public interface IMqttBroker {
        void Initialize(string ipAddress);

        bool TryConnect(string lwtTopic, string lwtPayload);

        void Disconnect();

        bool TryPublish(string topic, string payload, byte qosLevel, bool isRetained);

        bool TrySubscribe(string topic);

        event PublishReceivedDelegate PublishReceived;
    }

    public delegate void PublishReceivedDelegate(object sender, PublishReceivedEventArgs e);

    public class PublishReceivedEventArgs {
        public string Topic { get; private set; }
        public string Payload { get; private set; }

        public PublishReceivedEventArgs(string topic, string payload) {
            Topic = topic;
            Payload = payload;
        }
    }
}
