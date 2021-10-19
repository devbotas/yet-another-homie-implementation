using System.Collections;

namespace DevBot9.Protocols.Homie {
    public class ClientPropertyMetadata {
        public string NodeId { get; set; } = "no-id";
        public string PropertyId { get; set; } = "no-id";
        public PropertyType PropertyType { get; set; } = PropertyType.State;
        public string Name { get; set; } = "no-name";
        public DataType DataType { get; set; } = DataType.Blank;
        public string Format { get; set; } = "";
        public Hashtable Tags { get; } = new Hashtable();
        public string Unit { get; set; } = "";
        public string InitialValue { get; set; } = "";

        public override string ToString() {
            return PropertyId + ":" + InitialValue;
        }

        public bool ValidateAndFix(ref ArrayList errorList, ref ArrayList warningList) {
            var isOk = true;

            if ((PropertyType == PropertyType.Command) && (InitialValue != "")) {
                warningList.Add($"{NodeId}/{PropertyId} property type is Command, but it has initial value set. Clearing it.");

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
                            errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, which is not a valid initial value for integer data type. Skipping this property entirely.");
                            isOk = false;
                        }

                        if (isOk) {
                            warningList.Add($"{NodeId}/{PropertyId} is originally of type integer, but it will now be converted to float.");

                            Tags.Add("Precision", "0");
                            DataType = DataType.Float;
                        }
                    }

                    if (isNotCommand && (Helpers.IsFloat(InitialValue) == false)) {
                        errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, which is not a valid initial value for float data type. Skipping this property entirely.");
                        isOk = false;
                    }
                    break;

                case DataType.Boolean:
                case DataType.Enum:
                    // Trying to convert boolean type to enum. Later, enum validation will simply run on this converted property.
                    if (DataType == DataType.Boolean) {
                        if (isNotCommand && (Helpers.TryParseBool(InitialValue, out _) == false)) {
                            errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, which is not a valid initial value for boolean data type. Skipping this property entirely.");
                            isOk = false;
                        }

                        if (isOk) {
                            warningList.Add($"{NodeId}/{PropertyId} is originally of type boolean, but it will now be converted to enum.");

                            Format = "false,true";
                            DataType = DataType.Enum;

                            if (Unit != "") {
                                warningList.Add($"{NodeId}/{PropertyId}/$unit attribute is {Unit}. Should be empty for boolean data type. Clearing it.");
                                Unit = "";
                            }
                        }

                    }

                    // From here, the save procedure will run on both enum and (former) boolean properties.
                    if (Format == "") {
                        errorList.Add($"{NodeId}/{PropertyId}/$format attribute is empty, which is not valid for enum data type. Skipping this property entirely.");
                        isOk = false;
                    }

                    var options = Format.Split(',');

                    if (isOk) {
                        if (options.Length < 2) {
                            errorList.Add($"{NodeId}/{PropertyId}/$format attribute contains less than two option. Skipping this property entirely.");
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
                            errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, while it should be one of {Format}. Skipping this property entirely.");
                            isOk = false;
                        }
                    }
                    break;

                case DataType.Color:
                    if (Format == "") {
                        errorList.Add($"{NodeId}/{PropertyId}/$format attribute is empty, which is not valid for color data type. Skipping this property entirely.");
                        isOk = false;
                    }

                    if (isOk) {
                        if (Helpers.TryParseHomieColorFormat(Format, out var colorFormat) == false) {
                            errorList.Add($"{NodeId}/{PropertyId}/$format attribute is {Format}, which is not valid for color data type. Skipping this property entirely.");
                            isOk = false;
                        }
                        else if (isNotCommand) {
                            if (HomieColor.ValidatePayload(InitialValue, colorFormat) == false) {
                                errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, which is not valid for color format {colorFormat}. Skipping this property entirely.");
                                isOk = false;
                            }
                        }
                    }
                    break;

                case DataType.DateTime:
                    if (isNotCommand && (Helpers.TryParseDateTime(InitialValue, out var _) == false)) {
                        errorList.Add($"{NodeId}/{PropertyId} is set to {InitialValue}, which is not a valid initial value for datetime data type. Skipping this property entirely.");
                        isOk = false;
                    }
                    if (Unit != "") {
                        warningList.Add($"{NodeId}/{PropertyId}/$unit attribute is {Unit}. Should be empty for datetime data type. Clearing it.");
                        Unit = "";
                    }
                    if (Format != "") {
                        warningList.Add($"{NodeId}/{PropertyId}/$format attribute is {Unit}. Should be empty for datetime data type. Clearing it.");
                        Format = "";
                    }
                    break;

                case DataType.Duration:
                    errorList.Add($"{NodeId}/{PropertyId} is of type {DataType}, but this is not currently supported by YAHI. Skipping this property entirely.");
                    isOk = false;
                    break;
            }

            return isOk;
        }
    }
}
