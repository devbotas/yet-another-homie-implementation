namespace DevBot9.Protocols.Homie;

/// <summary>
/// This interface appends <see cref="ClientDevice"/> connection to suit it for <see cref="HostDevice"/> connection.
/// Every <see cref="HostDevice"/> must get a separate connection, or there will be malfunctions.
/// <see cref="HostDevice"/> will set the Will at some time during its initialization, so this must be taken into account when implementing this interface.
/// </summary>
public interface IHostDeviceConnection : IBasicDeviceConnection {
    void SetWill(string willTopic, string willPayload);
}
