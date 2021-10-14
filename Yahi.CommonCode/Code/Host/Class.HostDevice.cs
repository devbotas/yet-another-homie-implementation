using System;
using System.Collections;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Host Device implementation. It should be used to create a Homie Device that is not yet actually present on the MQTT broker.
    /// </summary>
    public class HostDevice : Device {
        #region Public interface

        /// <summary>
        /// Initializes the entire Host Device tree: actually creates internal property variables, publishes to topics and so on. This method must be called, or otherwise entire Host Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Host Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Host Device will not work.</param>
        public void Initialize(IHostDeviceConnection broker, AddToLogDelegate loggingFunction = null) {
            broker.SetWill(_willTopic, _willPayload);
            base.Initialize(broker, loggingFunction);

            _broker.Connected += (sender, e) => {
                // Need to republish all relevant topics, because broker may not have them retained (for example, when broker boots for the first time).
                var clonedPublishTable = (Hashtable)_publishedTopics.Clone();
                LogInfo($"{DeviceId}: Publishing all the the cached topics, of which there are: {clonedPublishTable.Count}.");
                foreach (string key in clonedPublishTable.Keys) {
                    _broker.Publish(key, (string)clonedPublishTable[key], 1, true);
                }

                // Publishing state at the very end.
                LogInfo($"{DeviceId}: Restoring state to {State}.");
                InternalPropertyPublish("$state", State.ToHomiePayload());
            };

            // One can pretty much do anything while in "init" state.
            SetState(HomieState.Init);

            // Initializing properties. They will start using broker immediatelly.
            foreach (HostPropertyBase property in _properties) {
                property.Initialize(this);
            }

            // Building node subtree.
            var nodesList = "";
            foreach (NodeInfo node in _nodes) {
                InternalPropertyPublish($"{node.Id}/$name", node.Name);
                InternalPropertyPublish($"{node.Id}/$type", node.Type);
                InternalPropertyPublish($"{node.Id}/$properties", node.Properties);

                nodesList += "," + node.Id;
            }
            nodesList = nodesList.Substring(1, nodesList.Length - 1);

            // The order in which these master properties are published may be important for the property discovery implementation.
            // I have a feeling that OpenHAB, HomeAssistant and other do actually behave differently if I rearrange properties below,
            // but the exact order is not specified in the convention...
            InternalPropertyPublish("$homie", HomieVersion);
            InternalPropertyPublish("$name", Name);
            InternalPropertyPublish("$nodes", nodesList);

            // Off we go. At this point discovery services should rebuild their trees.
            SetState(HomieState.Ready);

            _broker.Connect();
        }

        /// <summary>
        /// Set the state of the Device. Homie convention defines these states: init, ready, disconnected, sleeping, lost, alert.
        /// </summary>
        public void SetState(HomieState stateToSet) {
            State = stateToSet;
            InternalPropertyPublish("$state", State.ToHomiePayload());
        }

        /// <summary>
        /// Creates and/or updates the node for the Device. Nodes are somewhat virtual in Homie, you can create them at any time when build your Device tree, as long as <see cref="Initialize(PublishToTopicDelegate, SubscribeToTopicDelegate)"/> is not called yet.
        /// </summary>
        public void UpdateNodeInfo(string nodeId, string friendlyName, string type) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }

            // Trying to find if that's an existing node.
            NodeInfo nodeToUpdate = null;
            for (var i = 0; i < _nodes.Count; i++) {
                var currentNode = (NodeInfo)_nodes[i];
                if (currentNode.Id == nodeId) { nodeToUpdate = currentNode; }
            }

            // If not found - add new. Otherwise - update.
            if (nodeToUpdate == null) {
                nodeToUpdate = new NodeInfo() { Id = nodeId, Name = friendlyName, Type = type };
                _nodes.Add(nodeToUpdate);
            }
            else {
                nodeToUpdate.Name = friendlyName;
                nodeToUpdate.Type = type;
            }
        }

        /// <summary>
        /// Creates a host number property.
        /// </summary>
        public HostNumberProperty CreateHostNumberProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, double initialValue = 0.0, string unit = "", int decimalPlaces = 2) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostNumberProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, decimalPlaces, unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host text property.
        /// </summary>
        public HostTextProperty CreateHostTextProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string initialValue = "", string unit = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostTextProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host color property.
        /// </summary>
        public HostColorProperty CreateHostColorProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, ColorFormat colorFormat = ColorFormat.Rgb) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostColorProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, colorFormat, "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host choice property.
        /// </summary>
        public HostChoiceProperty CreateHostChoiceProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, in string[] possibleValues, string initialValue = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostChoiceProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, possibleValues, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host date and time property.
        /// </summary>
        public HostDateTimeProperty CreateHostDateTimeProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, DateTime initialValue) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostDateTimeProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        #endregion

        #region Private stuff

        private readonly ArrayList _nodes = new ArrayList();
        private Hashtable _publishedTopics = new Hashtable();
        private string _willTopic = "";
        private string _willPayload = "";
        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _willTopic = $"{baseTopic}/{id}/$state";
            _willPayload = "lost";
            DeviceId = id;
            Name = friendlyName;
            State = HomieState.Init;
        }

        internal override void InternalPropertyPublish(string propertyTopic, string value, bool isRetained = true) {
            var fullTopicId = $"{_baseTopic}/{DeviceId}/{propertyTopic}";

            if (isRetained) {
                // All the other topics are cached, because those will be (re)published on broker connection event.
                if (_publishedTopics.Contains(fullTopicId) == false) {
                    _publishedTopics.Add(fullTopicId, value);
                }
                else {
                    _publishedTopics[fullTopicId] = value;
                }
            }

            InternalGeneralPublish(fullTopicId, value);
        }

        private void UpdateNodePropertyMap(string nodeId, string propertyId) {
            // Trying to find if that's an existing node.
            NodeInfo nodeToUpdate = null;
            for (var i = 0; i < _nodes.Count; i++) {
                var currentNode = (NodeInfo)_nodes[i];
                if (currentNode.Id == nodeId) { nodeToUpdate = currentNode; }
            }

            // If not found - add new.
            if (nodeToUpdate == null) {
                nodeToUpdate = new NodeInfo() { Id = nodeId };
                _nodes.Add(nodeToUpdate);
            }

            nodeToUpdate.AddProperty(propertyId);
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

        #endregion
    }
}
