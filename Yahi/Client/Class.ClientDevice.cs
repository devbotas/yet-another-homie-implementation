﻿using System;
using System.Collections.Generic;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Client Device implementation. It should be used to consume a Homie Device that is already present on the MQTT broker.
    /// </summary>
    public class ClientDevice : Device {
        #region Public interface

        /// <summary>
        /// Initializes the entire Client Device tree: actually creates internal property variables, subscribes to topics and so on. This method must be called, or otherwise entire Client Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Client Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Client Device will not work.</param>
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            var homieTopic = $"{_baseTopic}/{_deviceId}/$homie";
            _topicHandlerMap.Add(homieTopic, new List<Action<string>>());
            _topicHandlerMap[homieTopic].Add((value) => {
                HomieVersion = value;
            });
            _subscribeToTopicDelegate(homieTopic);

            var nameTopic = $"{_baseTopic}/{_deviceId}/$name";
            _topicHandlerMap.Add(nameTopic, new List<Action<string>>());
            _topicHandlerMap[nameTopic].Add((value) => {
                Name = value;
            });
            _subscribeToTopicDelegate(nameTopic);

            var stateTopic = $"{_baseTopic}/{_deviceId}/$state";
            _topicHandlerMap.Add(stateTopic, new List<Action<string>>());
            _topicHandlerMap[stateTopic].Add((value) => {
                State = value;
            });
            _subscribeToTopicDelegate(stateTopic);
        }

        /// <summary>
        /// Creates a client string property.
        /// </summary>
        public ClientStringProperty CreateClientStringProperty(PropertyType propertyType, string nodeId, string propertyId) {
            var createdProperty = new ClientStringProperty(propertyType, $"{nodeId}/{propertyId}");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client integer property.
        /// </summary>
        public ClientIntegerProperty CreateClientIntegerProperty(PropertyType propertyType, string nodeId, string propertyId) {
            var createdProperty = new ClientIntegerProperty(propertyType, $"{nodeId}/{propertyId}");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client float property.
        /// </summary>
        public ClientFloatProperty CreateClientFloatProperty(PropertyType propertyType, string nodeId, string propertyId) {
            var createdProperty = new ClientFloatProperty(propertyType, $"{nodeId}/{propertyId}");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client boolean property.
        /// </summary>
        public ClientBooleanProperty CreateClientBooleanProperty(PropertyType propertyType, string nodeId, string propertyId) {
            var createdProperty = new ClientBooleanProperty(propertyType, $"{nodeId}/{propertyId}");

            _properties.Add(createdProperty);

            return createdProperty;
        }
        #endregion

        #region Private stuff

        internal ClientDevice(string baseTopic, string id) {
            _baseTopic = baseTopic;
            _deviceId = id;
        }

        #endregion
    }
}
