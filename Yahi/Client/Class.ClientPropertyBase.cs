using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class ClientPropertyBase {
        protected string _propertyId;
        protected Device _parentDevice;

        private string _name = "";
        public string Name {
            get { return _name; }
            protected set {
                _name = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private DataType _dataType = DataType.String;
        public DataType DataType {
            get { return _dataType; }
            protected set {
                _dataType = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        private string _format = "";
        public string Format {
            get { return _format; }
            protected set {
                _format = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        private string _unit = "";
        public string Unit {
            get { return _unit; }
            protected set {
                _unit = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Unit)));
            }
        }

        private string _value = "";
        public string Value {
            get { return _value; }
            protected set {
                _value = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected ClientPropertyBase(string propertyId) {
            _propertyId = propertyId;
        }

        protected void Initialize(Device parentDevice) {
            _parentDevice = parentDevice;

            _parentDevice.InternalPropertySubsribe($"{_propertyId}/$name", (value) => {
                Name = value;
            });

            _parentDevice.InternalPropertySubsribe($"{_propertyId}/$datatype", (value) => {
                DataType = DataType.String;
            });

            _parentDevice.InternalPropertySubsribe($"{_propertyId}/$format", (value) => {
                Format = value;
            });

            _parentDevice.InternalPropertySubsribe($"{_propertyId}/$unit", (value) => {
                Unit = value;
            });

            _parentDevice.InternalPropertySubsribe($"{_propertyId}", (value) => {
                Value = value;
            });
        }
    }
}
