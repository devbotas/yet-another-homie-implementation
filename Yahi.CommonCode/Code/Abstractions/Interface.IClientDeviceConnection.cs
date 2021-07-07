using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Used to wrap around concrete broker implementations (like M2Mqtt or MQTT.net) so that <see cref="ClientDevice"/> class could consume it.
    /// This connection may be shared between multiple <see cref="ClientDevice"/> instances.
    /// </summary>
    public interface IBasicDeviceConnection : INotifyPropertyChanged {
        bool IsConnected { get; }

        bool TryPublish(string topic, string payload, byte qosLevel, bool isRetained);
        bool TrySubscribe(string topic);

        event PublishReceivedDelegate PublishReceived;
    }

    public interface IClientDeviceConnection : IBasicDeviceConnection { }
}
