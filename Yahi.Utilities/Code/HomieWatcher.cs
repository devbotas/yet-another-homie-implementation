namespace DevBot9.Protocols.Homie.Utilities;

public record class DeviceUpdatedEventArgs(string DeviceId, HomieState NewState);

public class HomieWatcher {
    private readonly ChannelConnectionOptions _channelConnectionOptions = new();
    private readonly ConcurrentDictionary<string, ClientDevice> _devices = new();
    private readonly YahiTevuxClientConnection _yahiClient = new();
    private readonly Dictionary<string, string> _responses = new();
    private bool _isConnecting;
    private DateTime _timeOfLastUniqueTopic = DateTime.Now;
    private HomieWatcher() {
        _yahiClient.PublishReceived += HandleClientConnectionPublishReceived;
    }

    public event EventHandler<DeviceUpdatedEventArgs> DeviceUpdated = delegate { };

    public static HomieWatcher Instance { get; } = new HomieWatcher();
    public bool IsConnected => _yahiClient.IsConnected;
    public void Connect(string ipAddress) {
        _isConnecting = true;

        _channelConnectionOptions.SetHostname(ipAddress);

        DeviceFactory.Initialize("homie");

        // Consumers can share a single connection to the broker.
        _yahiClient.Initialize(_channelConnectionOptions);
        _yahiClient.Connect();
        _yahiClient.SubscribeAndWait("homie/#", QosLevel.AtLeastOnce);

        while ((DateTime.Now - _timeOfLastUniqueTopic).TotalMilliseconds < 500) {
            Thread.Sleep(100);
        }

        var topicDump = _responses.Select(r => r.Key + ":" + r.Value).ToArray();
        var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var _, out var _);

        foreach (var device in homieTree) {
            RebuildDevice(device.Id);
        }

        _isConnecting = false;
    }

    public void Disconnect() {
        _yahiClient.DisconnectAndWait();

        foreach (var device in _devices) {
            DeviceUpdated(this, new DeviceUpdatedEventArgs(device.Key, device.Value.State));
        }
    }

    public bool TryGetClientDevice(string deviceId, out ClientDevice clientDevice) {
        var deviceFound = false;

        if (_devices.ContainsKey(deviceId)) {
            clientDevice = _devices[deviceId];
            deviceFound = true;
        } else {
            clientDevice = null!;
        }

        return deviceFound;
    }

    public bool TryGetClientProperty(string deviceId, string nodeId, string propertyId, out ClientPropertyBase property, out HomieState state) {
        var propertyFound = false;
        property = null!;
        state = HomieState.Lost;

        if (TryGetClientDevice(deviceId, out var clientDevice)) {
            var myNode = clientDevice.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (myNode != null) {
                var myProperty = myNode.Properties.FirstOrDefault(p => p.PropertyId == nodeId + "/" + propertyId);
                if (myProperty != null) {
                    property = myProperty;
                    state = clientDevice.State;
                    propertyFound = true;
                }
            }
        }

        return propertyFound;
    }
    private void HandleClientConnectionPublishReceived(object sender, DevBot9.Protocols.Homie.PublishReceivedEventArgs e) {
        var payload = e.Payload;

        var topicShallBeAdded = (_responses.ContainsKey(e.Topic) == false) && (payload != "");
        var topicShallBeRemoved = _responses.ContainsKey(e.Topic) && (payload == "");
        var topicShallBeUpdated = _responses.ContainsKey(e.Topic) && (payload != "");
        if (topicShallBeAdded) {
            _responses.Add(e.Topic, payload);
            _timeOfLastUniqueTopic = DateTime.Now;
        }
        if (topicShallBeUpdated) {
            _responses[e.Topic] = payload;
        }
        if (topicShallBeRemoved) {
            _responses.Remove(e.Topic);
        }

        if ((_isConnecting == false) && (e.Topic[^6..] == "$homie")) {
            var newDeviceId = e.Topic.Split('/')[1];
            RebuildDevice(newDeviceId);
        }

        if ((_isConnecting == false) && (e.Topic[^6..] == "$state")) {
            var newDeviceId = e.Topic.Split('/')[1];
            Helpers.TryParseHomieState(e.Payload, out var newState);
            DeviceUpdated(this, new DeviceUpdatedEventArgs(newDeviceId, newState));
        }
    }

    private void HandleDevicePropertyChangedEvent(object? sender, PropertyChangedEventArgs e) {
        if (sender is ClientDevice device) {
            if (e.PropertyName == nameof(device.State)) {
                DeviceUpdated(this, new DeviceUpdatedEventArgs(device.DeviceId, device.State));
            }
        }
    }

    private void RebuildDevice(string deviceId) {
        var deviceDump = _responses.Where(r => r.Key.StartsWith(DeviceFactory.BaseTopic + "/" + deviceId)).Select(r => r.Key + ":" + r.Value).ToArray();
        var newDeviceTree = HomieTopicTreeParser.Parse(deviceDump, DeviceFactory.BaseTopic, out var _, out var _);

        var deviceShallBeRemoved = false;
        var deviceShallBeAdded = false;
        var deviceShallBeReplaced = false;

        if (_devices.ContainsKey(deviceId) && (newDeviceTree.Length == 0)) {
            deviceShallBeRemoved = true;
        }

        if ((_devices.ContainsKey(deviceId) == false) && (newDeviceTree.Length == 1)) {
            deviceShallBeAdded = true;
        }

        if (_devices.ContainsKey(deviceId) && (newDeviceTree.Length == 1)) {
            var oldDevice = _devices[deviceId];
            if (Helpers.TryParseHomieState(newDeviceTree[0].StateAttribute, out var newState)) {
                if (oldDevice.State != newState) {
                    deviceShallBeReplaced = true;
                }
            }
        }

        if (deviceShallBeRemoved || deviceShallBeReplaced) {
            _devices.Remove(deviceId, out var removedDevice);
            removedDevice!.PropertyChanged -= HandleDevicePropertyChangedEvent;
            removedDevice.Dispose();
            DeviceUpdated(this, new DeviceUpdatedEventArgs(removedDevice.DeviceId, removedDevice.State));
        }

        if (deviceShallBeAdded || deviceShallBeReplaced) {
            var newDevice = DeviceFactory.CreateClientDevice(newDeviceTree[0]);
            _devices[deviceId] = newDevice;
            newDevice.Initialize(_yahiClient);
            newDevice.PropertyChanged += HandleDevicePropertyChangedEvent;
            DeviceUpdated(this, new DeviceUpdatedEventArgs(newDevice.DeviceId, newDevice.State));
        }
    }
}
