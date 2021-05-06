using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A base class for the Client properties. Should not be consumed directly.
    /// </summary>
    public class ClientPropertyBase : PropertyBase {
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

        protected ClientPropertyBase(ClientPropertyMetadata creationOptions) {
            Name = creationOptions.Name;
            Type = creationOptions.PropertyType;
            _propertyId = creationOptions.NodeId + "/" + creationOptions.PropertyId;
            DataType = creationOptions.DataType;

            if (Type == PropertyType.Command) {
                // Discarding initial value, it there was any. These are not applicable to commands.
                _rawValue = "";
            }

            switch (DataType) {
                case DataType.String:
                    Format = "";
                    Unit = "";
                    break;

                case DataType.Integer:
                    Format = "";
                    Unit = creationOptions.Unit;
                    break;

                case DataType.Float:
                    Format = creationOptions.Format;
                    Unit = creationOptions.Unit;
                    break;

                case DataType.Boolean:
                    Format = "";
                    Unit = "";
                    break;

                case DataType.Enum:
                    var possibleValues = creationOptions.Format.Split(',');
                    if (possibleValues.Length < 2) { throw new ArgumentException("Please provide at least two possible values for this property.", nameof(creationOptions.Format)); }

                    if (Type != PropertyType.Command) {
                        if (string.IsNullOrEmpty(creationOptions.InitialValue)) { _rawValue = possibleValues[0]; }
                        else {
                            var isMatchFound = false;
                            foreach (var value in possibleValues) {
                                if (value == creationOptions.InitialValue) { isMatchFound = true; }
                            }

                            if (isMatchFound == false) { throw new ArgumentException("Initial value is not one of the possible values", nameof(creationOptions.InitialValue)); }
                        }
                    }

                    Format = creationOptions.Format;
                    Unit = "";
                    break;

                case DataType.Color:
                    Format = creationOptions.Format;
                    Unit = "";
                    break;

                case DataType.DateTime:
                    Format = "";
                    Unit = "";
                    break;

                case DataType.Duration:
                    Format = "";
                    Unit = "";
                    break;
            }
        }

        internal override void Initialize(Device parentDevice) {
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
        protected virtual bool ValidatePayload(string payloadToValidate) {
            return false;
        }
    }
}
