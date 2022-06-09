using System;
using System.Text;
using Tevux.Protocols.Mqtt;

namespace DevBot9.Protocols.Homie.Utilities;

public class YahiTevuxClientConnection : MqttClient, IClientDeviceConnection {
    private bool _isInitialized = false;
    private ChannelConnectionOptions _channelConnectionOptions = new();
    protected MqttConnectionOptions _mqttConnectionOptions = new();

    public new event PublishReceivedDelegate PublishReceived = delegate { };
    public new EventHandler Connected;
    public new EventHandler Disconnected;

    public void Initialize(ChannelConnectionOptions channelConnectionOptions) {
        _channelConnectionOptions = channelConnectionOptions;

        if (_isInitialized) { return; }

        Initialize();

        base.PublishReceived += (sender, e) => {
            PublishReceived(this, new PublishReceivedEventArgs(e.Topic, Encoding.UTF8.GetString(e.Message)));
        };

        _isInitialized = true;
    }

    public void Connect() {
        ConnectAndWait(_channelConnectionOptions, _mqttConnectionOptions);
    }

    public void Publish(string topic, string payload, byte qosLevel, bool isRetained) {
        Publish(topic, Encoding.UTF8.GetBytes(payload), (QosLevel)qosLevel, isRetained);
    }

    public void Subscribe(string topic) {
        Subscribe(topic, QosLevel.AtLeastOnce);
    }
    public void Subscribe(string topic, bool isConfirmationRequired = false) {
        if (isConfirmationRequired) {
            SubscribeAndWait(topic, QosLevel.AtLeastOnce);
        }
        else {
            Subscribe(topic, QosLevel.AtLeastOnce);
        }
    }
}