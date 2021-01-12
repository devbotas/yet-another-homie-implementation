using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class ClientPropertyBase {
        protected IBroker _broker;
        protected string _topicPrefix;
        protected string _propertyId;

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

        protected ClientPropertyBase(string topicPrefix, string propertyId) {
            _topicPrefix = topicPrefix;
            _propertyId = propertyId;
        }

        protected void Initialize(IBroker broker) {
            _broker = broker;

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$name", (topic, value) => {
                Name = value;
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$datatype", (topic, value) => {
                DataType = DataType.String;
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$format", (topic, value) => {
                Format = value;
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}/$unit", (topic, value) => {
                Unit = value;
            });

            _broker.Subscribe($"{_topicPrefix}/{_propertyId}", (topic, value) => {
                Value = value;
            });
        }
    }
}
