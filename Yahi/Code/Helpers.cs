using System.Globalization;
using System;
using System.Text.RegularExpressions;

namespace DevBot9.Protocols.Homie;

public static class Helpers {

    #region Parsers

    public static double ParseDouble(string stringToParse) {
        double returnValue;

        returnValue = double.Parse(stringToParse, CultureInfo.InvariantCulture);

        return returnValue;
    }

    public static bool TryParseDouble(string stringToParse, out double result) {
        var returnValue = false;

        returnValue = double.TryParse(stringToParse, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        return returnValue;
    }

    public static bool TryParseInt(string stringToParse, out int result) {
        var parseSucceeded = false;

        parseSucceeded = int.TryParse(stringToParse, out result);

        return parseSucceeded;
    }
    public static bool ParseBool(string stringToParse) {
        var returnValue = false;

        returnValue = bool.Parse(stringToParse);

        return returnValue;
    }
    public static bool TryParseBool(string stringToParse, out bool result) {
        var parseSucceeded = false;

        parseSucceeded = bool.TryParse(stringToParse, out result);

        return parseSucceeded;
    }

    public static DateTime ParseDateTime(string stringToParse) {
        var returnValue = DateTime.UtcNow;

        returnValue = DateTime.Parse(stringToParse);

        return returnValue;
    }

    public static bool TryParseDateTime(string stringToParse, out DateTime result) {
        var parseSucceeded = false;

        parseSucceeded = DateTime.TryParse(stringToParse, out result);

        return parseSucceeded;
    }

    public static bool TryParseHomieDataType(string stringToParse, out DataType parsedType) {
        var isParsed = true;
        switch (stringToParse) {
            case "string":
                parsedType = DataType.String;
                break;

            case "integer":
                parsedType = DataType.Integer;
                break;

            case "float":
                parsedType = DataType.Float;
                break;

            case "boolean":
                parsedType = DataType.Boolean;
                break;

            case "enum":
                parsedType = DataType.Enum;
                break;

            case "color":
                parsedType = DataType.Color;
                break;

            case "datetime":
                parsedType = DataType.DateTime;
                break;

            case "duration":
                parsedType = DataType.Duration;
                break;

            default:
                parsedType = DataType.String;
                isParsed = false;
                break;
        }

        return isParsed;
    }

    public static bool TryParseHomieState(string stringToParse, out HomieState parsedState) {
        var isParsed = true;
        switch (stringToParse) {
            case "init":
                parsedState = HomieState.Init;
                break;

            case "ready":
                parsedState = HomieState.Ready;
                break;

            case "disconnected":
                parsedState = HomieState.Disconnected;
                break;

            case "sleeping":
                parsedState = HomieState.Sleeping;
                break;

            case "lost":
                parsedState = HomieState.Lost;
                break;

            case "alert":
                parsedState = HomieState.Alert;
                break;

            default:
                parsedState = HomieState.Lost;
                isParsed = false;
                break;
        }

        return isParsed;
    }

    public static bool TryParseHomieColorFormat(string stringToParse, out ColorFormat parsedFormat) {
        var isParsed = true;
        switch (stringToParse) {
            case "rgb":
                parsedFormat = ColorFormat.Rgb;
                break;

            case "hsv":
                parsedFormat = ColorFormat.Hsv;
                break;

            default:
                parsedFormat = ColorFormat.Rgb;
                isParsed = false;
                break;
        }

        return isParsed;
    }

    #endregion

    #region String helpers

    public static string GetDoubleFormatString(int decimalPlaces) {
        if (decimalPlaces > 7) { decimalPlaces = 7; }
        if (decimalPlaces < 0) { decimalPlaces = 0; }

        var returnFormat = $"F{decimalPlaces}";

        return returnFormat;
    }

    public static string DoubleToString(double numberToConvert, string format = "") {
        string returnValue;

        if (format != "") { returnValue = numberToConvert.ToString(format, CultureInfo.InvariantCulture); }
        else { returnValue = numberToConvert.ToString(CultureInfo.InvariantCulture); }

        return returnValue;
    }

    public static bool StartsWith(this string originalString, string stringToCheck) {
        var returnValue = false;

        if (string.IsNullOrEmpty(originalString)) { returnValue = false; }
        if (string.IsNullOrEmpty(stringToCheck)) { returnValue = false; }
        if (stringToCheck.Length > originalString.Length) { returnValue = false; }

        if (originalString[..stringToCheck.Length] == stringToCheck) { returnValue = true; }

        return returnValue;
    }

    #endregion

    #region Enum extensions
    // Two reasons for those: first, bigNET and nanoNET have different ToString() implementations.
    // bigNET returns enum's name, nanoNET return value as a number. Second, it generally helps,
    // as there is some letter capitality to deal with.
    public static string ToHomiePayload(this DataType dataType) {
        var returnValue = "";

        switch (dataType) {
            case DataType.String:
                returnValue = "string";
                break;

            case DataType.Integer:
                returnValue = "integer";
                break;

            case DataType.Float:
                returnValue = "float";
                break;

            case DataType.Boolean:
                returnValue = "boolean";
                break;

            case DataType.Enum:
                returnValue = "enum";
                break;

            case DataType.Color:
                returnValue = "color";
                break;

            case DataType.DateTime:
                returnValue = "datetime";
                break;

            case DataType.Duration:
                returnValue = "duration";
                break;
        }

        return returnValue;
    }

    public static string ToHomiePayload(this HomieState homieState) {
        var returnValue = "";

        switch (homieState) {
            case HomieState.Init:
                returnValue = "init";
                break;

            case HomieState.Ready:
                returnValue = "ready";
                break;

            case HomieState.Disconnected:
                returnValue = "disconnected";
                break;

            case HomieState.Sleeping:
                returnValue = "sleeping";
                break;

            case HomieState.Lost:
                returnValue = "lost";
                break;

            case HomieState.Alert:
                returnValue = "alert";
                break;
        }

        return returnValue;
    }

    public static string ToHomiePayload(this ColorFormat colorFormat) {
        var returnValue = "";

        switch (colorFormat) {
            case ColorFormat.Rgb:
                returnValue = "rgb";
                break;

            case ColorFormat.Hsv:
                returnValue = "hsv";
                break;
        }

        return returnValue;
    }

    public static string ToHomiePayload(this bool booleanValue) {
        var returnValue = booleanValue ? "true" : "false";

        return returnValue;
    }

    public static string ToHomiePayload(this int intValue) {
        var returnValue = intValue.ToString();

        return returnValue;
    }

    #endregion

    #region RegEx helpers

    public static bool IsInteger(string value) {
        var integerRegex = new Regex("^[+-]?([0]|[1-9]+[0-9]*)$");

        var isInteger = integerRegex.IsMatch(value);

        return isInteger;
    }

    public static bool IsFloat(string value) {
        var simpleDecimalRegex = new Regex("^[+-]?([0]|[1-9]+[0-9]*)([.][0-9]+)?$");
        var standardNotationRegex = new Regex("^[-+]?[0-9][.][0-9]+([eE][-+]?[0-9]+)$");

        var isFloat = simpleDecimalRegex.IsMatch(value) || standardNotationRegex.IsMatch(value);

        return isFloat;
    }

    #endregion
}
