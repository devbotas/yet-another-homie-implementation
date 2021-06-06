﻿using System;
using System.Collections;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Client Device implementation. It should be used to consume a Homie Device that is already present on the MQTT broker.
    /// </summary>
    public class ClientDevice : Device {
        #region Public interface
        public ClientNode[] Nodes;


        /// <summary>
        /// Initializes the entire Client Device tree: actually creates internal property variables, subscribes to topics and so on. This method must be called, or otherwise entire Client Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Client Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Client Device will not work.</param>
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            var homieTopic = $"{_baseTopic}/{_deviceId}/$homie";
            _topicHandlerMap.Add(homieTopic, new ArrayList());
            ActionString handler = delegate (string value) {
                HomieVersion = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(HomieVersion)));
            };
            ((ArrayList)_topicHandlerMap[homieTopic]).Add(handler);
            _subscribeToTopicDelegate(homieTopic);

            var nameTopic = $"{_baseTopic}/{_deviceId}/$name";
            _topicHandlerMap.Add(nameTopic, new ArrayList());
            ActionString handler2 = delegate (string value) {
                Name = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            };
            ((ArrayList)_topicHandlerMap[nameTopic]).Add(handler);
            _subscribeToTopicDelegate(nameTopic);

            var stateTopic = $"{_baseTopic}/{_deviceId}/$state";
            _topicHandlerMap.Add(stateTopic, new ArrayList());
            ActionString handler3 = delegate (string value) {
                if (Helpers.TryParseHomieState(value, out var parsedState)) {
                    State = parsedState;
                    RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(State)));
                };
            };
            ((ArrayList)_topicHandlerMap[stateTopic]).Add(handler);
            _subscribeToTopicDelegate(stateTopic);
        }

        /// <summary>
        /// Creates a client string property.
        /// </summary>
        public ClientStringProperty CreateClientStringProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.String; }
            else if (creationOptions.DataType != DataType.String) { throw new ArgumentException($"You're creating a {nameof(ClientStringProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }


            var createdProperty = new ClientStringProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client integer property.
        /// </summary>
        public ClientIntegerProperty CreateClientIntegerProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Integer; }
            else if (creationOptions.DataType != DataType.Integer) { throw new ArgumentException($"You're creating a {nameof(ClientIntegerProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }


            var createdProperty = new ClientIntegerProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client float property.
        /// </summary>
        public ClientFloatProperty CreateClientFloatProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Float; }
            else if (creationOptions.DataType != DataType.Float) { throw new ArgumentException($"You're creating a {nameof(CreateClientFloatProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }


            var createdProperty = new ClientFloatProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client boolean property.
        /// </summary>
        public ClientBooleanProperty CreateClientBooleanProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Boolean; }
            else if (creationOptions.DataType != DataType.Boolean) { throw new ArgumentException($"You're creating a {nameof(ClientBooleanProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            var createdProperty = new ClientBooleanProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client enum property.
        /// </summary>
        public ClientEnumProperty CreateClientEnumProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Enum; }
            else if (creationOptions.DataType != DataType.Enum) { throw new ArgumentException($"You're creating a {nameof(ClientEnumProperty)}, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            var createdProperty = new ClientEnumProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client color property.
        /// </summary>
        public ClientColorProperty CreateClientColorProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Color; }
            else if (creationOptions.DataType != DataType.Color) { throw new ArgumentException($"You're creating a {nameof(ClientColorProperty)}, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            var createdProperty = new ClientColorProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }
        #endregion

        #region Private stuff

        internal ClientDevice(string baseTopic, string id) {
            _baseTopic = baseTopic;
            _deviceId = id;
        }

        internal ClientDevice(string baseTopic, ClientDeviceMetadata deviceMetadata) {
            _baseTopic = baseTopic;
            _deviceId = deviceMetadata.Id;
            Name = deviceMetadata.NameAttribute;

            if (Helpers.TryParseHomieState(deviceMetadata.StateAttribute, out var parsedState)) {
                State = parsedState;
            }
            else {
                State = HomieState.Lost;
            }

            Nodes = new ClientNode[deviceMetadata.Nodes.Length];

            for (var n = 0; n < deviceMetadata.Nodes.Length; n++) {
                var nodeMetaData = deviceMetadata.Nodes[n];
                var node = new ClientNode();
                Nodes[n] = node;

                node.Name = nodeMetaData.NameAttribute;
                node.Type = nodeMetaData.TypeAttribute;
                node.Properties = new ClientPropertyBase[nodeMetaData.Properties.Length];

                for (var p = 0; p < nodeMetaData.Properties.Length; p++) {
                    var propertyMetadata = nodeMetaData.Properties[p];


                    switch (propertyMetadata.DataType) {
                        case DataType.Integer:
                            var newIntegerProperty = CreateClientIntegerProperty(propertyMetadata);
                            node.Properties[p] = newIntegerProperty;
                            break;

                        case DataType.Float:
                            var newFloatProperty = CreateClientFloatProperty(propertyMetadata);
                            node.Properties[p] = newFloatProperty;
                            break;

                        case DataType.Boolean:
                            var newBooleanProperty = CreateClientBooleanProperty(propertyMetadata);
                            node.Properties[p] = newBooleanProperty;
                            break;

                        case DataType.Enum:
                            var newEnumProperty = CreateClientEnumProperty(propertyMetadata);
                            node.Properties[p] = newEnumProperty;
                            break;


                        case DataType.Color:
                            var newColorProperty = CreateClientColorProperty(propertyMetadata);
                            node.Properties[p] = newColorProperty;
                            break;


                        case DataType.DateTime:
                            // Now Datetime cannot be just displayed as string property. Data types are checked internally, and an exception is thrown if when trying create a StringProperty with data type DateTime.
                            break;

                        case DataType.String:
                            var newStringProperty = CreateClientStringProperty(propertyMetadata);
                            node.Properties[p] = newStringProperty;
                            break;

                    }
                }
            }
        }

        #endregion
    }
}
