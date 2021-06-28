using System;
using System.Collections;
using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Client Device implementation. It should be used to consume a Homie Device that is already present on the MQTT broker.
    /// </summary>
    public class ClientDevice : Device {
        #region Public interface
        /// <summary>
        /// Device Nodes, as defined by the Homie convention.
        /// </summary>
        public ClientNode[] Nodes { get; private set; } = new ClientNode[0];

        /// <summary>
        /// Initializes the entire Client Device tree: actually creates internal property variables, subscribes to topics and so on. This method must be called, or otherwise entire Client Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Client Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Client Device will not work.</param>
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            var homieTopic = $"{_baseTopic}/{DeviceId}/$homie";
            _topicHandlerMap.Add(homieTopic, new ArrayList());
            ActionString handler = delegate (string value) {
                HomieVersion = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(HomieVersion)));
            };
            ((ArrayList)_topicHandlerMap[homieTopic]).Add(handler);
            _subscribeToTopicDelegate(homieTopic);

            var nameTopic = $"{_baseTopic}/{DeviceId}/$name";
            _topicHandlerMap.Add(nameTopic, new ArrayList());
            ActionString handler2 = delegate (string value) {
                Name = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            };
            ((ArrayList)_topicHandlerMap[nameTopic]).Add(handler2);
            _subscribeToTopicDelegate(nameTopic);

            var stateTopic = $"{_baseTopic}/{DeviceId}/$state";
            _topicHandlerMap.Add(stateTopic, new ArrayList());
            ActionString handler3 = delegate (string value) {
                if (Helpers.TryParseHomieState(value, out var parsedState)) {
                    State = parsedState;
                    RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(State)));
                };
            };
            ((ArrayList)_topicHandlerMap[stateTopic]).Add(handler3);
            _subscribeToTopicDelegate(stateTopic);
        }

        /// <summary>
        /// Creates a client string property.
        /// </summary>
        public ClientTextProperty CreateClientTextProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.String; }
            else if (creationOptions.DataType != DataType.String) { throw new ArgumentException($"You're creating a {nameof(ClientTextProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }


            var createdProperty = new ClientTextProperty(creationOptions);

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
        public ClientNumberProperty CreateClientNumberProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Float; }
            else if (creationOptions.DataType != DataType.Float) { throw new ArgumentException($"You're creating a {nameof(CreateClientNumberProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }


            var createdProperty = new ClientNumberProperty(creationOptions);

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
        public ClientChoiceProperty CreateClientChoiceProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Enum; }
            else if (creationOptions.DataType != DataType.Enum) { throw new ArgumentException($"You're creating a {nameof(ClientChoiceProperty)}, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            var createdProperty = new ClientChoiceProperty(creationOptions);

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
            DeviceId = id;
        }

        internal ClientDevice(string baseTopic, ClientDeviceMetadata deviceMetadata) {
            _baseTopic = baseTopic;
            DeviceId = deviceMetadata.Id;
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
                            var newFloatProperty = CreateClientNumberProperty(propertyMetadata);
                            node.Properties[p] = newFloatProperty;
                            break;

                        case DataType.Boolean:
                            var newBooleanProperty = CreateClientBooleanProperty(propertyMetadata);
                            node.Properties[p] = newBooleanProperty;
                            break;

                        case DataType.Enum:
                            var newEnumProperty = CreateClientChoiceProperty(propertyMetadata);
                            node.Properties[p] = newEnumProperty;
                            break;


                        case DataType.Color:
                            var newColorProperty = CreateClientColorProperty(propertyMetadata);
                            node.Properties[p] = newColorProperty;
                            break;


                        case DataType.DateTime:
#warning cannot parse DateTime at this moment, because nF dosn't have parsing methods, and I kinda don't want to implement them myself... Yhus, converting this property into a string for now.
                            propertyMetadata.DataType = DataType.String;
                            var newDateTimeProperty = CreateClientTextProperty(propertyMetadata);
                            node.Properties[p] = newDateTimeProperty;
                            break;

                        case DataType.String:
                            var newStringProperty = CreateClientTextProperty(propertyMetadata);
                            node.Properties[p] = newStringProperty;
                            break;

                    }
                }
            }
        }

        #endregion
    }
}
