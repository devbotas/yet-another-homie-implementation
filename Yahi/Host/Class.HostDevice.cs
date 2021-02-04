using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace DevBot9.Protocols.Homie {
    public class HostDevice : Device {
        //        private Dictionary<string, List<string>> _nodeProperties = new Dictionary<string, List<string>>();
        private List<NodeInfo> _nodes = new List<NodeInfo>();

        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _deviceId = id;
            Name = friendlyName;
            State = States.Init;
        }

        public HostIntegerProperty CreateHostIntegerProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostIntegerProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, DataType.Integer, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        public HostFloatProperty CreateHostFloatProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostFloatProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, DataType.Float, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        public HostStringProperty CreateHostStringProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostStringProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, DataType.String, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        public HostBooleanProperty CreateHostBooleanProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName) {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostBooleanProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, DataType.Boolean, "", "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            SetState(States.Init);


            var nodesList = "";

            foreach (var node in _nodes) {
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$name", node.Name);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$type", node.Type);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$properties", node.Properties);

                nodesList += "," + node.Id;
            }

            nodesList = nodesList.Substring(1, nodesList.Length - 1);


            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$name", Name);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$nodes", nodesList);
            //_client.Publish($"homie/{_deviceId}/$extensions", GetExtensionsString());

            // imitating some initialization work.
            Thread.Sleep(1000);

            SetState(States.Ready);
        }

        public void SetState(string stateToSet) {
            State = stateToSet;
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$state", State);
        }

        public void UpdateNodeInfo(string nodeId, string friendlyName, string type) {
            if (_nodes.Any(n => n.Id == nodeId) == false) {
                _nodes.Add(new NodeInfo() { Id = nodeId, Name = friendlyName, Type = type });
            }

            var nodeToUpdate = _nodes.First(n => n.Id == nodeId);
            nodeToUpdate.Name = friendlyName;
            nodeToUpdate.Type = type;
        }

        private void UpdateNodePropertyMap(string nodeId, string propertyId) {
            if (_nodes.Any(n => n.Id == nodeId) == false) {
                _nodes.Add(new NodeInfo() { Id = nodeId });
            }
            //if (_nodeProperties.ContainsKey(nodeId) == false) {
            //    _nodeProperties.Add(nodeId, new List<string>());
            //}

            var nodeToUpdate = _nodes.First(n => n.Id == nodeId);
            nodeToUpdate.AddProperty(propertyId);

            // _nodeProperties[nodeId].Add(propertyId);
        }

        private class NodeInfo {
            internal string Id { get; set; } = "no-id";
            internal string Name { get; set; } = "no-name";
            internal string Type { get; set; } = "no-type";
            public string Properties { get; private set; } = "";

            internal void AddProperty(string propertyId) {
                if (Properties == "") {
                    Properties = propertyId;
                }
                else {
                    Properties += "," + propertyId;
                }
            }
        }
    }
}
