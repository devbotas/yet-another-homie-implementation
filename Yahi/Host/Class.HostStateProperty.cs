namespace DevBot9.Protocols.Homie {
    public class HostStateProperty : HostPropertyBase {
        internal HostStateProperty(string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, false, true, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        public void SetValue(string valueToSet) {
            // Deliberately setting a protected field. I do not want to raise PropertyUpdated event,
            // because I'm modifying it from inside. Event is when external client modifies the Value,
            // that is, sends an external command.
            _value.SetValue(valueToSet);

            _parentDevice.InternalPropertyPublish($"{_propertyId}", Value.GetStringValue());
        }

        public void SetValue(object valueToSet) {
            var mqttPayload = "";

            switch (_dataTypeAttribute) {
                case DataType.String:
                    mqttPayload = valueToSet.ToString();
                    break;

                case DataType.Integer:
                    if (valueToSet is int) {
                        mqttPayload = ((int)valueToSet).ToString();
                    }
                    if (valueToSet is long) {
                        mqttPayload = ((long)valueToSet).ToString();
                    }
                    if (valueToSet is float) {
                        mqttPayload = ((float)valueToSet).ToString();
                    }
                    if (valueToSet is double) {
                        mqttPayload = ((double)valueToSet).ToString();
                    }
                    break;

                case DataType.Float:
                    if (valueToSet is int) {
                        mqttPayload = ((int)valueToSet).ToString();
                    }
                    if (valueToSet is long) {
                        mqttPayload = ((long)valueToSet).ToString();
                    }
                    if (valueToSet is float) {
                        mqttPayload = ((float)valueToSet).ToString();
                    }
                    if (valueToSet is double) {
                        mqttPayload = ((double)valueToSet).ToString();
                    }
                    break;

                case DataType.Percent:
                    if (valueToSet is int) {
                        mqttPayload = ((int)valueToSet).ToString();
                    }
                    if (valueToSet is long) {
                        mqttPayload = ((long)valueToSet).ToString();
                    }
                    if (valueToSet is float) {
                        mqttPayload = ((float)valueToSet).ToString();
                    }
                    if (valueToSet is double) {
                        mqttPayload = ((double)valueToSet).ToString();
                    }
                    break;

                case DataType.Boolean:

                    break;

                case DataType.Enum:

                    break;

                case DataType.Color:

                    break;

                case DataType.DateTime:

                    break;

                case DataType.Duration:

                    break;

            }





            // _value = valueToSet;

            _parentDevice.InternalPropertyPublish($"{_propertyId}", mqttPayload);
        }
    }
}
