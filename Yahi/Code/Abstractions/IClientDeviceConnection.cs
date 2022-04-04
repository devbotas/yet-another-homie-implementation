using System;

namespace DevBot9.Protocols.Homie;

/// <summary>
/// Used to wrap around concrete broker implementations (like M2Mqtt or MQTT.net) so that <see cref="ClientDevice"/> class could consume it.
/// This connection may be shared between multiple <see cref="ClientDevice"/> instances.
/// </summary>
public interface IBasicDeviceConnection {
    bool IsConnected { get; }

    event PublishReceivedDelegate PublishReceived;
    event EventHandler Connected;
    event EventHandler Disconnected;

    void Connect();
    void Publish(string topic, string payload, byte qosLevel, bool isRetained);
    void Subscribe(string topic);
}

public interface IClientDeviceConnection : IBasicDeviceConnection { }
