using System;

namespace DevBot9.Protocols.Homie {
    public class HomieValue {
        private object _internalValue;
        private string _stringValue;
        private int _intValue;
        private float _floatValue;

        private string _regexStringForInt64 = @"^-?\d{1,19}$"; // <-- It may actually overflow, need a better string.
        private string _regexStringForSimpleFloat = @"[-+]?[0-9]*\.?[0-9]+";
        private string _regexStringForExponentFloat = @"[-+]?[0-9]{1}\.?[0-9]+([eE][-+]?[0-9]+)?";

        public DataType DataType { get; private set; }

        public string GetStringValue() {
            return _stringValue;
        }

        public int GetIntValue() {
            if (DataType == DataType.Integer) {
                return _intValue;
            }
            else {
                throw new ArgumentException();
            }
        }

        public float GetFloatValue() {
            if (DataType == DataType.Float) {
                return _floatValue;
            }
            else {
                throw new ArgumentException();
            }
        }

        public void SetValue(string valueToSet) {
            if (DataType == DataType.String) {
                _stringValue = valueToSet;
            }
            else {
                throw new ArgumentException();
            }
        }
        public void SetValue(int valueToSet) {
            if (DataType == DataType.Integer) {
                _intValue = valueToSet;
            }
            else {
                throw new ArgumentException();
            }
        }
        public void SetValue(float valueToSet) {
            if (DataType == DataType.Float) {
                _floatValue = valueToSet;
            }
            else {
                throw new ArgumentException();
            }
        }

        public static implicit operator int(HomieValue v) {
            return v.GetIntValue();
        }
        public static implicit operator float(HomieValue v) {
            return v.GetFloatValue();
        }
    }
}
