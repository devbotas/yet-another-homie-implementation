namespace DevBot9.Protocols.Homie;

/// <summary>
/// A property of type Enum, as defined by the Homie convention.
/// </summary>
public class ClientChoiceProperty : ClientPropertyBase {
    private readonly string[] _possibleValues = Array.Empty<string>();

    /// <summary>
    /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
    /// </summary>
    public string Value {
        get {
            var returnValue = _rawValue;

            return returnValue;
        }
        set {
            SetValue(value);
        }
    }

    internal ClientChoiceProperty(ClientPropertyMetadata creationProperties) : base(creationProperties) {
        _possibleValues = creationProperties.Format.Split(',');
    }

    protected override bool ValidatePayload(string payloadToValidate) {
        var returnValue = false;

        foreach (var option in _possibleValues) {
            if (payloadToValidate == option) {
                returnValue = true;
            }
        }

        return returnValue;
    }

    private void SetValue(string valueToSet) {
        switch (Type) {
            case PropertyType.Parameter:
            case PropertyType.Command:
                if (ValidatePayload(valueToSet)) {
                    _rawValue = valueToSet;
                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                }
                else {
                    throw new ArgumentOutOfRangeException($"Parameter value \"{valueToSet}\" is not permitted.");
                }
                break;

            case PropertyType.State:
                throw new InvalidOperationException();
        }
    }
}
