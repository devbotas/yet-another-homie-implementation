using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A base class for the Client properties. Should not be consumed directly.
    /// </summary>
    public class ClientPropertyBase : INotifyPropertyChanged {
        /// <summary>
        /// Event is raised when the property is changed externally, that is, when an update is received from the MQTT broker.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// ID of the property.
        /// </summary>
        public string PropertyId {
            get {
                return _propertyId;
            }
        }

        /// <summary>
        /// Logical type of the property. This is NOT defined by Homie convention, but rather and additional constrain added by YAHI. However, it is fully Homie-compliant.
        /// </summary>
        public PropertyType Type { get; protected set; } = PropertyType.State;

        /// <summary>
        /// Friendly name of the property.
        /// </summary>
        public string Name {
            get { return _name; }
            protected set {
                _name = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        /// <summary>
        /// Data type of the property, as defined by the Homie convention.
        /// </summary>
        public DataType DataType {
            get { return _dataType; }
            protected set {
                _dataType = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        /// <summary>
        /// Format of the property, as (inconsistently) defined by the Homie convention.
        /// </summary>
        public string Format {
            get { return _format; }
            protected set {
                _format = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        /// <summary>
        /// Unit of the property, as (softly) defined by the Homie convention.
        /// </summary>
        public string Unit {
            get { return _unit; }
            protected set {
                _unit = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Unit)));
            }
        }

        private string _name = "";
        private DataType _dataType = DataType.String;
        private string _format = "";
        private string _unit = "";
        protected string _rawValue = "";
        protected string _propertyId;
        protected ClientDevice _parentDevice;

        protected ClientPropertyBase(ClientPropertyMetadata creationOptions) {
            Name = creationOptions.Name;
            Type = creationOptions.PropertyType;
            _propertyId = creationOptions.NodeId + "/" + creationOptions.PropertyId;
            DataType = creationOptions.DataType;
            Format = creationOptions.Format;
            Unit = creationOptions.Unit;
            _rawValue = creationOptions.InitialValue;
        }

        internal void Initialize(ClientDevice parentDevice) {
            _parentDevice = parentDevice;

            if (Type == PropertyType.State) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }
        }

        internal void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged(sender, e);
        }

        protected virtual bool ValidatePayload(string payloadToValidate) {
            return false;
        }
    }
}
