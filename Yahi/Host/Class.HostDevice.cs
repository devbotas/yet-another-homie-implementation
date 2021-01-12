using System.Collections.Generic;
using System.Threading;
namespace DevBot9.Protocols.Homie {
    public class HostDevice {
        string _baseTopic = "temp";
        string _deviceId = "some-device";
        IBroker _broker;

        List<HostStateProperty> _stateProperties = new List<HostStateProperty>();
        List<HostCommandProperty> _commandProperties = new List<HostCommandProperty>();
        List<HostParameterProperty> _parameterProperties = new List<HostParameterProperty>();

        public string HomieVersion { get; } = "4.0.0";
        public string Name { get; private set; }
        public string State { get; private set; }

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
                property.Initialize(_broker);
            }
            foreach (var property in _commandProperties) {
                property.Initialize(_broker);
            }
            foreach (var property in _parameterProperties) {
                property.Initialize(_broker);
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
