using System;

namespace DevBot9.Protocols.Homie;

/// <summary>
/// This class provides Homie Color property implementation. It can transparently convert between HSV and RGB types of data.
/// </summary>
public class HomieColor {
    /// <summary>
    /// Red component, ranging from 0 to 255.
    /// </summary>
    public double RedValue { get; private set; } = 0;

    /// <summary>
    /// Green component, ranging from 0 to 255.
    /// </summary>
    public double GreenValue { get; private set; } = 0;

    /// <summary>
    /// Blue component, ranging from 0 to 255.
    /// </summary>
    public double BlueValue { get; private set; } = 0;

    /// <summary>
    /// Creates a new HomieColor from an RGB string.
    /// </summary>
    public static HomieColor FromRgbString(string rgbString) {
        HomieColor returnColor;

        if (ValidatePayload(rgbString, ColorFormat.Rgb)) {
            returnColor = new HomieColor();

            var colorParts = rgbString.Split(',');

            returnColor.RedValue = int.Parse(colorParts[0]);
            returnColor.GreenValue = int.Parse(colorParts[1]);
            returnColor.BlueValue = int.Parse(colorParts[2]);
        }
        else {
            returnColor = new HomieColor();
        }

        return returnColor;
    }

    /// <summary>
    /// Creates a new HomieColor from an HSV string.
    /// </summary>
    public static HomieColor FromHsvString(string hsvString) {
        HomieColor returnColor;

        if (ValidatePayload(hsvString, ColorFormat.Hsv)) {
            returnColor = new HomieColor();

            var colorParts = hsvString.Split(',');

            var hue = int.Parse(colorParts[0]);
            var saturation = int.Parse(colorParts[1]) / 100.0;
            var value = int.Parse(colorParts[2]) / 100.0;

            // Took conversion from Wikipedia https://en.wikipedia.org/wiki/HSL_and_HSV
            var chroma = saturation * value;
            var hPrime = hue / 60.0;
            var x = chroma * (1 - Math.Abs(hPrime % 2 - 1));

            var redPrime = 0.0;
            var greenPrime = 0.0;
            var bluePrime = 0.0;
            if (hPrime <= 1) {
                redPrime = chroma;
                greenPrime = x;
                bluePrime = 0;
            }
            else if (hPrime <= 2) {
                redPrime = x;
                greenPrime = chroma;
                bluePrime = 0;
            }
            else if (hPrime <= 3) {
                redPrime = 0;
                greenPrime = chroma;
                bluePrime = x;
            }
            else if (hPrime <= 4) {
                redPrime = 0;
                greenPrime = x;
                bluePrime = chroma;
            }
            else if (hPrime <= 5) {
                redPrime = x;
                greenPrime = 0;
                bluePrime = chroma;
            }
            else if (hPrime <= 6) {
                redPrime = chroma;
                greenPrime = 0;
                bluePrime = x;
            }

            var m = value - chroma;

            returnColor.RedValue = (redPrime + m) * 255;
            returnColor.GreenValue = (greenPrime + m) * 255;
            returnColor.BlueValue = (bluePrime + m) * 255;
        }
        else {
            returnColor = new HomieColor();
        }

        return returnColor;
    }

    /// <summary>
    /// Returns color in RGB string format.
    /// </summary>
    public string ToRgbString() {
        var returnColor = $"{RedValue},{GreenValue},{BlueValue}";

        return returnColor;

    }

    /// <summary>
    /// Returns color in HSV string format.
    /// </summary>
    public string ToHsvString() {
        var xMax = Math.Max(RedValue, GreenValue);
        xMax = Math.Max(xMax, BlueValue);

        var xMin = Math.Min(RedValue, GreenValue);
        xMin = Math.Min(xMin, BlueValue);

        var chroma = xMax - xMin;
        var lightness = (xMax + xMin) / 2;
        var v = xMax;

        var hue = 0.0;
        var saturation = 0.0;
        var value = v;

        if (chroma == 0) {
            hue = 0;
        }
        if (v == RedValue) {
            hue = 60 * (0 + (GreenValue - BlueValue) / chroma);
        }
        if (v == GreenValue) {
            hue = 60 * (2 + (BlueValue - RedValue) / chroma);
        }
        if (v == BlueValue) {
            hue = 60 * (4 + (RedValue - GreenValue) / chroma);
        }

        if (v == 0) { saturation = 0; }
        else { saturation = chroma / v; }

        return $"{hue},{saturation},{value}";
    }

    /// <summary>
    /// Validates if a string conforms to Homie color format (RGB or HSV).
    /// </summary>
    public static bool ValidatePayload(string payloadToValidate, ColorFormat colorFormat) {
        var colorParts = payloadToValidate.Split(',');
        if (colorParts.Length != 3) { return false; }

        var areNumbersGood = true;
        if (colorFormat == ColorFormat.Rgb) {
            if (Helpers.TryParseInt(colorParts[0], out var red)) {
                if (red < 0) { areNumbersGood &= false; }
                if (red > 255) { areNumbersGood &= false; }
            };
            if (Helpers.TryParseInt(colorParts[1], out var green)) {
                if (green < 0) { areNumbersGood &= false; }
                if (green > 255) { areNumbersGood &= false; }
            };
            if (Helpers.TryParseInt(colorParts[2], out var blue)) {
                if (blue < 0) { areNumbersGood &= false; }
                if (blue > 255) { areNumbersGood &= false; }
            }
        }
        if (colorFormat == ColorFormat.Hsv) {
            if (Helpers.TryParseInt(colorParts[0], out var hue)) {
                if (hue < 0) { areNumbersGood &= false; }
                if (hue > 360) { areNumbersGood &= false; }
            };
            if (Helpers.TryParseInt(colorParts[1], out var saturation)) {
                if (saturation < 0) { areNumbersGood &= false; }
                if (saturation > 100) { areNumbersGood &= false; }
            };
            if (Helpers.TryParseInt(colorParts[2], out var value)) {
                if (value < 0) { areNumbersGood &= false; }
                if (value > 100) { areNumbersGood &= false; }
            }
        }

        return areNumbersGood;
    }

    public static HomieColor CreateBlack() {
        return FromRgbString("0,0,0");
    }

    private HomieColor() {
        // Just so no one calls constructor accidentally.
    }
}
