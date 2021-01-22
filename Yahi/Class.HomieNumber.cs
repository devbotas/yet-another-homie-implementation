namespace DevBot9.Protocols.Homie {
    public struct HomieNumber {
        public decimal Value;
        public string Format;

        public HomieNumber(int value, string format = "0.00") {
            Value = new decimal(value);
            Format = format;
        }
        public HomieNumber(double value, string format = "0.00") {
            Value = new decimal(value);
            Format = format;
        }

        public static implicit operator HomieNumber(int value) {
            var returnValue = new HomieNumber(value);

            return returnValue;
        }

        public static implicit operator HomieNumber(double value) {
            var returnValue = new HomieNumber(value);

            return returnValue;
        }

        public static implicit operator int(HomieNumber v) {
            var returnValue = 0;

            returnValue = decimal.ToInt32(v.Value);

            return returnValue;
        }

        public static implicit operator double(HomieNumber v) {
            var returnValue = 0.0;

            returnValue = decimal.ToDouble(v.Value);

            return returnValue;
        }

        public static implicit operator float(HomieNumber v) {
            var returnValue = 0.0f;

            returnValue = decimal.ToSingle(v.Value);

            return returnValue;
        }

        public static HomieNumber operator *(HomieNumber a, HomieNumber b) {
            var returnValue = new HomieNumber();

            returnValue.Value = decimal.Multiply(a.Value, b.Value);
            returnValue.Format = a.Format;

            return returnValue;
        }

        public override string ToString() {
            return Value.ToString(Format);

        }
    }
}
