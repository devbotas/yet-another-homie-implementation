using System.Collections;

namespace DevBot9.Protocols.Homie {
    public class ClientPropertyMetadata {
        public string NodeId { get; set; } = "no-id";
        public string PropertyId { get; set; } = "no-id";
        public PropertyType PropertyType { get; set; } = PropertyType.State;
        public string Name { get; set; } = "no-name";
        public DataType DataType { get; set; } = DataType.Blank;
        public string Format { get; set; } = "";
        public string Unit { get; set; } = "";
        public string InitialValue { get; set; } = "";

        public override string ToString() {
            return PropertyId + ":" + InitialValue;
        }

        public bool ValidateAndFix(ref ArrayList problemList) {
            var isOk = true;

            if ((PropertyType == PropertyType.Command) && (InitialValue != "")) {
                problemList.Add($"Warning:{NodeId}.{PropertyId} property type is Command, but it has initial value set. Clearing it.");

                InitialValue = "";
            }

            var isNotCommand = PropertyType != PropertyType.Command;
            switch (DataType) {
                case DataType.String:
                    // String type has pretty much no restriction to attributes content.
                    break;

                case DataType.Integer:
                case DataType.Float:
                    // Trying to convert integer to float. Later, float validation will simply run on this converted property.
                    if (DataType == DataType.Integer) {
                        if (isNotCommand && (Helpers.IsInteger(InitialValue) == false)) {
                            problemList.Add($"Error:{NodeId}.{PropertyId} is set to {InitialValue}, which is not a valid initial value for integer data type. Skipping this property entirely.");
                            isOk = false;
                        }

                        if (isOk) {
                            problemList.Add($"Warning:{NodeId}.{PropertyId} is originally of type integer, but it will now be converted to float.");

                            Format = "F0";
                            DataType = DataType.Float;
                        }
                    }

                    if (isNotCommand && (Helpers.IsFloat(InitialValue) == false)) {
                        problemList.Add($"Error:{NodeId}.{PropertyId} is set to {InitialValue}, which is not a valid initial value for float data type. Skipping this property entirely.");
                        isOk = false;
                    }
                    break;

                case DataType.Boolean:
                case DataType.Enum:
                    // Trying to convert boolean type to enum. Later, enum validation will simply run on this converted property.
                    if (DataType == DataType.Boolean) {
                        if (isNotCommand && (Helpers.TryParseBool(InitialValue, out _) == false)) {
                            problemList.Add($"Error:{NodeId}.{PropertyId} is set to {InitialValue}, which is not a valid initial value for boolean data type. Skipping this property entirely.");
                            isOk = false;
                        }

                        if (isOk) {
                            problemList.Add($"Warning:{NodeId}.{PropertyId} is originally of type boolean, but it will now be converted to enum.");

                            Format = "false,true";
                            DataType = DataType.Enum;

                            if (Unit != "") {
                                problemList.Add($"Warning:{NodeId}.{PropertyId}.$unit attribute is {Unit}. Should be empty for boolean data type. Clearing it.");
                                Unit = "";
                            }
                        }

                    }

                    // From here, the save procedure will run on both enum and (former) boolean properties.
                    if (Format == "") {
                        problemList.Add($"Warning:{NodeId}.{PropertyId}.$format attribute is empty, which is not valid for enum data type. Skipping this property entirely.");
                        isOk = false;
                    }

                    var options = Format.Split(',');

                    if (isOk) {
                        if (options.Length < 2) {
                            problemList.Add($"Error:{NodeId}.{PropertyId}.$format attribute contains less than two option. Skipping this property entirely.");
                            isOk = false;
                        }
                    }

                    if (isOk) {
                        var isInitialValueCorrect = false;
                        foreach (var option in options) {
                            if (InitialValue == option) {
                                isInitialValueCorrect = true;
                            }
                        }

                        if (isNotCommand && (isInitialValueCorrect == false)) {
                            problemList.Add($"Error:{NodeId}.{PropertyId} is set to {InitialValue}, while it should be one of {Format}. Skipping this property entirely.");
                            isOk = false;
                        }
                    }
                    break;

                case DataType.Color:
                    if (Format == "") {
                        problemList.Add($"Warning:{NodeId}.{PropertyId}.$format attribute is empty, which is not valid for color data type. Skipping this property entirely.");
                        isOk = false;
                    }

                    if (isOk) {
                        if (Helpers.TryParseHomieColorFormat(Format, out var colorFormat) == false) {
                            problemList.Add($"Warning:{NodeId}.{PropertyId}.$format attribute is {Format}, which is not valid for color data type. Skipping this property entirely.");
                            isOk = false;
                        }
                        else if (isNotCommand) {
                            if (HomieColor.ValidatePayload(InitialValue, colorFormat) == false) {
                                problemList.Add($"Error:{NodeId}.{PropertyId} is set to {InitialValue}, which is not valid for color format {colorFormat}. Skipping this property entirely.");
                                isOk = false;
                            }
                        }
                    }
                    break;

                case DataType.DateTime:
                case DataType.Duration:
                    problemList.Add($"Error:{NodeId}.{PropertyId} is of type {DataType}, but this is not currently supported by YAHI. Skipping this property entirely.");
                    isOk = false;
                    break;
            }

            return isOk;
        }
    }
}
