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
        public void Initialize(IClientDeviceConnection broker, AddToLogDelegate loggingFunction = null) {
            base.Initialize(broker, loggingFunction);

            // Initializing properties. They will start using broker immediatelly.
            foreach (ClientPropertyBase property in _properties) {
                property.Initialize(this);
            }

            var homieTopic = $"{_baseTopic}/{DeviceId}/$homie";
            ActionStringDelegate handlerForTopicHomie = delegate (string value) {
                HomieVersion = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(HomieVersion)));
            };
            InternalGeneralSubscribe(homieTopic, handlerForTopicHomie);

            var nameTopic = $"{_baseTopic}/{DeviceId}/$name";
            ActionStringDelegate handlerForTopicName = delegate (string value) {
                Name = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            };
            InternalGeneralSubscribe(nameTopic, handlerForTopicName);

            var stateTopic = $"{_baseTopic}/{DeviceId}/$state";
            ActionStringDelegate handlerForTopicState = delegate (string value) {
                if (Helpers.TryParseHomieState(value, out var parsedState)) {
                    State = parsedState;
                    RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(State)));
                };
            };
            InternalGeneralSubscribe(stateTopic, handlerForTopicState);
        }

        /// <summary>
        /// Creates a client text property.
        /// </summary>
        public ClientTextProperty CreateClientTextProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.String; }
            if (creationOptions.DataType != DataType.String) { throw new ArgumentException($"You're creating a {nameof(ClientTextProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            CheckForValidityAndThrowIfSomethingIsWrong(creationOptions);

            var createdProperty = new ClientTextProperty(creationOptions);
            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client number property.
        /// </summary>
        public ClientNumberProperty CreateClientNumberProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Float; }
            if (creationOptions.DataType != DataType.Float) { throw new ArgumentException($"You're creating a {nameof(CreateClientNumberProperty)} property, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            CheckForValidityAndThrowIfSomethingIsWrong(creationOptions);

            var createdProperty = new ClientNumberProperty(creationOptions);
            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client choice property.
        /// </summary>
        public ClientChoiceProperty CreateClientChoiceProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Enum; }
            if (creationOptions.DataType != DataType.Enum) { throw new ArgumentException($"You're creating a {nameof(ClientChoiceProperty)}, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            CheckForValidityAndThrowIfSomethingIsWrong(creationOptions);

            var createdProperty = new ClientChoiceProperty(creationOptions);
            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client color property.
        /// </summary>
        public ClientColorProperty CreateClientColorProperty(ClientPropertyMetadata creationOptions) {
            if (creationOptions.DataType == DataType.Blank) { creationOptions.DataType = DataType.Color; }
            if (creationOptions.DataType != DataType.Color) { throw new ArgumentException($"You're creating a {nameof(ClientColorProperty)}, but type specified is {creationOptions.DataType}. Either set it correctly, or leave a default value (that is is, don't set it at all)."); }

            CheckForValidityAndThrowIfSomethingIsWrong(creationOptions);

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
                        case DataType.Float:
                            var newNumberProperty = CreateClientNumberProperty(propertyMetadata);
                            node.Properties[p] = newNumberProperty;
                            break;

                        case DataType.Boolean:
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

        private void CheckForValidityAndThrowIfSomethingIsWrong(ClientPropertyMetadata creationOptions) {
            var errorList = new ArrayList();
            var warningList = new ArrayList();

            var isMetadataOk = creationOptions.ValidateAndFix(ref errorList, ref warningList);

            if (isMetadataOk == false) {
                var errorMessage = $"Provided metadata is incorrect. Errors: ";
                foreach (var problem in errorList) {
                    errorMessage += problem + " ";
                }
                throw new ArgumentException(errorMessage);
            }
        }
        #endregion
    }
}
