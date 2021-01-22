using System.Threading;
namespace DevBot9.Protocols.Homie {
    public class HostDevice : Device {
        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _deviceId = id;
            Name = friendlyName;
            State = States.Init;
        }

        public HostIntegerProperty CreateHostIntegerProperty(PropertyType protertyType, string propertyId, string friendlyName, string unit) {
            var createdProperty = new HostIntegerProperty(protertyType, propertyId, friendlyName, DataType.Integer, "", unit);

            _stateProperties.Add(createdProperty);

            return createdProperty;
        }

        public HostFloatProperty CreateHostFloatProperty(PropertyType protertyType, string propertyId, string friendlyName, string unit) {
            var createdProperty = new HostFloatProperty(protertyType, propertyId, friendlyName, DataType.Float, "", unit);

            _stateProperties.Add(createdProperty);

            return createdProperty;
        }

        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            SetState(States.Init);

            _publishToTopicDelegate($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            _publishToTopicDelegate($"{_baseTopic}/{_deviceId}/$name", Name);
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
            _publishToTopicDelegate($"{_baseTopic}/{_deviceId}/$state", State);
        }

    }
}
