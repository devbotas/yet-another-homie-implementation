using System.Threading;
namespace DevBot9.Protocols.Homie {
    public class HostDevice : Device {
        IBroker _broker;

        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _deviceId = id;
            Name = friendlyName;
            State = States.Init;
        }

        public HostStateProperty CreateHostStateProperty(string propertyId, string friendlyName, DataType dataType, string unit) {
            var createdProperty = new HostStateProperty($"{_baseTopic}/{_deviceId}", propertyId, friendlyName, dataType, "", unit);

            _stateProperties.Add(createdProperty);

            return createdProperty;
        }

        public HostCommandProperty CreateHostCommandProperty(string propertyId, string friendlyName, DataType dataType, string unit) {
            var createdProperty = new HostCommandProperty($"{_baseTopic}/{_deviceId}", propertyId, friendlyName, dataType, "", unit);

            _commandProperties.Add(createdProperty);

            return createdProperty;
        }

        public HostParameterProperty CreateHostParameterProperty(string propertyId, string friendlyName, DataType dataType, string unit) {
            var createdProperty = new HostParameterProperty($"{_baseTopic}/{_deviceId}", propertyId, friendlyName, dataType, "", unit);

            _parameterProperties.Add(createdProperty);

            return createdProperty;
        }

        public void Initialize(IBroker broker) {
            _broker = broker;

            SetState(States.Init);

            _broker.Publish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            _broker.Publish($"{_baseTopic}/{_deviceId}/$name", Name);
            //_client.Publish($"homie/{_deviceId}/$nodes", GetNodesString());
            //_client.Publish($"homie/{_deviceId}/$extensions", GetExtensionsString());

            foreach (var property in _stateProperties) {
                property.Initialize(this);
            }
            foreach (var property in _commandProperties) {
                property.Initialize(this);
            }
            foreach (var property in _parameterProperties) {
                property.Initialize(this);
            }

            // imitating some initialization work.
            Thread.Sleep(1000);

            SetState(States.Ready);
        }


        public void SetState(string stateToSet) {
            State = stateToSet;
            _broker.Publish($"{_baseTopic}/{_deviceId}/$state", State);
        }

    }
}
