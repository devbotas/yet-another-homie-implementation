#if !NANOFRAMEWORK_1_0
using System.Globalization;
#endif

using System;

namespace DevBot9.Protocols.Homie {
    public static class Helpers {
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
            var returnValue = false;

#if !NANOFRAMEWORK_1_0
            returnValue = int.TryParse(stringToParse, out result);
#else
            try {
                result = int.Parse(stringToParse);
            }
            catch (System.Exception) {
                result = int.MinValue;
            }
#endif
            return returnValue;
        }
        public static bool ParseBool(string stringToParse) {
            var returnValue = false;

#if !NANOFRAMEWORK_1_0
            returnValue = bool.Parse(stringToParse);
#else
            if (stringToParse == bool.TrueString) { returnValue = true; }
            if (stringToParse == bool.FalseString) { returnValue = false; }
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
            var returnValue = false;

#if !NANOFRAMEWORK_1_0
            returnValue = DateTime.TryParse(stringToParse, out result);
#else
#warning Need to reimplement DateTime parsing
            returnValue = true;
            result = DateTime.UtcNow;

#endif
            return returnValue;
        }
        public static string FloatToString(float numberToConvert, string format = "0.0") {
            string returnValue;

#if !NANOFRAMEWORK_1_0
            returnValue = numberToConvert.ToString(format, CultureInfo.InvariantCulture);
#else
            returnValue = numberToConvert.ToString(format);
#endif
            return returnValue;
        }
    }
}
