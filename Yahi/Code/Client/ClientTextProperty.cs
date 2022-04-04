using System;

namespace DevBot9.Protocols.Homie;

/// <summary>
/// A property of type String, as defined by the Homie convention.
/// </summary>
public class ClientTextProperty : ClientPropertyBase {
    /// <summary>
    /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
    /// </summary>
    public string Value {
        get {
            return _rawValue;
        }
        set {
            SetValue(value);
        }
    }

    public ClientTextProperty(ClientPropertyMetadata creationOptions) : base(creationOptions) {

    }

    protected override bool ValidatePayload(string payloadToValidate) {
        return true;
    }

    private void SetValue(string valueToSet) {
        switch (Type) {
            case PropertyType.Parameter:
            case PropertyType.Command:
                _rawValue = valueToSet;
                _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue, false);
                break;

            case PropertyType.State:
                throw new InvalidOperationException();
        }
    }
}
