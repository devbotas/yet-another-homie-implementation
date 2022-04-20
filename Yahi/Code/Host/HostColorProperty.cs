namespace DevBot9.Protocols.Homie;

/// <summary>
/// A property of type Color, as defined by the Homie convention.
/// </summary>
public class HostColorProperty : HostPropertyBase {
    /// <summary>
    /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
    /// </summary>
    public HomieColor Value {
        get {
            var returnValue = HomieColor.CreateBlack();

            if (_format == ColorFormat.Rgb) { returnValue = HomieColor.FromRgbString(_rawValue); }
            if (_format == ColorFormat.Hsv) { returnValue = HomieColor.FromHsvString(_rawValue); }

            return returnValue;
        }
        set {
            SetValue(value);
        }
    }

    private readonly ColorFormat _format = ColorFormat.Rgb;

    internal HostColorProperty(PropertyType propertyType, string propertyId, string friendlyName, ColorFormat format, string unit) : base(propertyType, propertyId, friendlyName, DataType.Color, format.ToHomiePayload(), unit) {
        _rawValue = "0,0,0";
        _format = format;
    }

    protected override bool ValidatePayload(string payloadToValidate) {
        var areNumbersGood = HomieColor.ValidatePayload(payloadToValidate, _format);

        return areNumbersGood;
    }

    private void SetValue(HomieColor valueToSet) {
        switch (Type) {
            case PropertyType.State:
            case PropertyType.Parameter:
                if (_format == ColorFormat.Rgb) { _rawValue = valueToSet.ToRgbString(); }
                if (_format == ColorFormat.Hsv) { _rawValue = valueToSet.ToHsvString(); }

                _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                break;

            case PropertyType.Command:
                throw new InvalidOperationException();
        }
    }
}
