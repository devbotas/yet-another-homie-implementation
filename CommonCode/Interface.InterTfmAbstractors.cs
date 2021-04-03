#if !NANOFRAMEWORK_1_0
using System.Globalization;
#endif

using System;

namespace DevBot9.Protocols.Homie {
    public static class Helpers {

        #region Parsers

        public static float ParseFloat(string stringToParse) {
            float returnValue;

#if !NANOFRAMEWORK_1_0
            returnValue = float.Parse(stringToParse, CultureInfo.InvariantCulture);
#else
            returnValue = float.Parse(stringToParse);
#endif
            return returnValue;
        }

        public static bool TryParseFloat(string stringToParse, out float result) {
            var returnValue = false;

#if !NANOFRAMEWORK_1_0
            returnValue = float.TryParse(stringToParse, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
#else
            returnValue = float.TryParse(stringToParse, out result);
#endif
            return returnValue;
        }

        public static bool TryParseInt(string stringToParse, out int result) {
            var parseSucceeded = false;

#if !NANOFRAMEWORK_1_0
            parseSucceeded = int.TryParse(stringToParse, out result);
#else
            try {
                result = int.Parse(stringToParse);
                parseSucceeded = true;
            }
            catch (System.Exception) {
                result = int.MinValue;
            }
#endif
            return parseSucceeded;
        }
        public static bool ParseBool(string stringToParse) {
            var returnValue = false;

#if !NANOFRAMEWORK_1_0
            returnValue = bool.Parse(stringToParse);
#else
            if (stringToParse.ToLower() == bool.TrueString.ToLower()) { returnValue = true; }
            if (stringToParse.ToLower() == bool.FalseString.ToLower()) { returnValue = false; }
#endif
            return returnValue;
        }

        public static DateTime ParseDateTime(string stringToParse) {
            var returnValue = DateTime.UtcNow;

#if !NANOFRAMEWORK_1_0
            returnValue = DateTime.Parse(stringToParse);
#else
#warning Need to reimplement DateTime parsing

#endif
            return returnValue;
        }

        public static bool TryParseDateTime(string stringToParse, out DateTime result) {
            var parseSucceeded = false;

#if !NANOFRAMEWORK_1_0
            parseSucceeded = DateTime.TryParse(stringToParse, out result);
#else
#warning Need to reimplement DateTime parsing
            parseSucceeded = true;
            result = DateTime.UtcNow;

#endif
            return parseSucceeded;
        }

        #endregion

        public static string FloatToString(float numberToConvert, string format = "0.0") {
            string returnValue;

#if !NANOFRAMEWORK_1_0
            returnValue = numberToConvert.ToString(format, CultureInfo.InvariantCulture);
#else
            returnValue = numberToConvert.ToString(format);
#endif
            return returnValue;
        }

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

                case DataType.Percent:
                    returnValue = "percent";
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

        #endregion
    }
}
