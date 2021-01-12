using System.Collections.Generic;
using System.Diagnostics;

namespace DevBot9.Protocols.Homie {
    public class ClientDevice {
        string _baseTopic = "temp";
        string _deviceId = "some-device";
        IBroker _broker;

        List<ClientStateProperty> _stateProperties = new List<ClientStateProperty>();
        List<ClientCommandProperty> _commandProperties = new List<ClientCommandProperty>();
        List<ClientParameterProperty> _parameterProperties = new List<ClientParameterProperty>();

        public string HomieVersion { get; private set; } = "4.0.0";
        public string Name { get; private set; }
        public string State { get; private set; }

        internal ClientDevice(string baseTopic, string id) {
            _baseTopic = baseTopic;
            _deviceId = id;
        }

        public ClientStateProperty CreateClientStateProperty(string propertyId) {
            var createdProperty = new ClientStateProperty($"{_baseTopic}/{_deviceId}", propertyId);

            _stateProperties.Add(createdProperty);

            return createdProperty;
        }

        public ClientCommandProperty CreateClientCommandProperty(string propertyId) {
            var createdProperty = new ClientCommandProperty($"{_baseTopic}/{_deviceId}", propertyId);

            _commandProperties.Add(createdProperty);

            return createdProperty;
        }

        public ClientParameterProperty CreateClientParameterProperty(string propertyId) {
            var createdProperty = new ClientParameterProperty($"{_baseTopic}/{_deviceId}", propertyId);

            _parameterProperties.Add(createdProperty);

            return createdProperty;
        }

        public void Initialize(IBroker broker) {
            _broker = broker;

            foreach (var property in _stateProperties) {
                property.Initialize(_broker);
            }

            foreach (var property in _commandProperties) {
                property.Initialize(_broker);
            }
            foreach (var property in _parameterProperties) {
                property.Initialize(_broker);
            }

            _broker.Subscribe($"{_baseTopic}/{_deviceId}/$homie", (a, b) => {
                Debug.WriteLine($"{a}: {b}");
                HomieVersion = b;
            });
            _broker.Subscribe($"{_baseTopic}/{_deviceId}/$name", (a, b) => {
                Debug.WriteLine($"{a}: {b}");
                Name = b;
            });
            _broker.Subscribe($"{_baseTopic}/{_deviceId}/$state", (a, b) => {
                Debug.WriteLine($"{a}: {b}");
                State = b;
            });

            //_broker.Publish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            //_broker.Publish($"{_baseTopic}/{_deviceId}/$name", Name);
            //_client.Publish($"homie/{_deviceId}/$nodes", GetNodesString());
            //_client.Publish($"homie/{_deviceId}/$extensions", GetExtensionsString());
        }
    }
}
