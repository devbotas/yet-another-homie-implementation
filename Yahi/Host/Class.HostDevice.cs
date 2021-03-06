﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            // This will initialize all the properties.
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            // One can pretty much do anything while in "init" state.
            SetState(States.Init);

            // Building node subtree.
            var nodesList = "";
            foreach (var node in _nodes) {
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$name", node.Name);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$type", node.Type);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$properties", node.Properties);

                nodesList += "," + node.Id;
            }
            nodesList = nodesList.Substring(1, nodesList.Length - 1);

            // The order in which these master properties are published may be important for the property discovery implementation.
            // I have a feeling that OpenHAB, HomeAssistant and other do actually behave differently if I rearrange properties below,
            // but the exact order is not specified in the convention...
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$name", Name);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$nodes", nodesList);

            // imitating some initialization work.
            Thread.Sleep(1000);

            // Off we go. At this point discovery services should rebuild their trees.
            SetState(States.Ready);
        }

        /// <summary>
        /// Set the state of the Device. Homie convention defines these states: init, ready, disconnected, sleeping, lost, alert.
        /// </summary>
        public void SetState(string stateToSet) {
#warning make an enum maybe?..
            State = stateToSet;
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$state", State);
        }

        /// <summary>
        /// Creates and/or updates the node for the Device. Nodes are somewhat virtual in Homie, you can create them at any time when build your Device tree, as long as <see cref="Initialize(PublishToTopicDelegate, SubscribeToTopicDelegate)"/> is not called yet.
        /// </summary>
        public void UpdateNodeInfo(string nodeId, string friendlyName, string type) {
            if (_nodes.Any(n => n.Id == nodeId) == false) {
                _nodes.Add(new NodeInfo() { Id = nodeId, Name = friendlyName, Type = type });
            }

            var nodeToUpdate = _nodes.First(n => n.Id == nodeId);
            nodeToUpdate.Name = friendlyName;
            nodeToUpdate.Type = type;
        }

        /// <summary>
        /// Creates a host integer property.
        /// </summary>
        public HostIntegerProperty CreateHostIntegerProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, int initialValue = 0, string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostIntegerProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host float property.
        /// </summary>
        public HostFloatProperty CreateHostFloatProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, float initialValue = 0.0f, string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostFloatProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host string property.
        /// </summary>
        public HostStringProperty CreateHostStringProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string initialValue = "", string unit = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostStringProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host boolean property.
        /// </summary>
        public HostBooleanProperty CreateHostBooleanProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, bool initialValue = false) {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostBooleanProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host color property.
        /// </summary>
        public HostColorProperty CreateHostColorProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, ColorFormat colorFormat = ColorFormat.Rgb) {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostColorProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, colorFormat, "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host enum property.
        /// </summary>
        public HostEnumProperty CreateHostEnumProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, in string[] possibleValues, string initialValue = "") {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostEnumProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, possibleValues, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host date ant time property.
        /// </summary>
        public HostDateTimeProperty CreateHostDateTimeProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, DateTime initialValue) {
            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostDateTimeProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        #endregion

        #region Private stuff

        private List<NodeInfo> _nodes = new List<NodeInfo>();

        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _deviceId = id;
            Name = friendlyName;
            State = States.Init;
        }


        private void UpdateNodePropertyMap(string nodeId, string propertyId) {
            if (_nodes.Any(n => n.Id == nodeId) == false) {
                _nodes.Add(new NodeInfo() { Id = nodeId });
            }

            var nodeToUpdate = _nodes.First(n => n.Id == nodeId);
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
