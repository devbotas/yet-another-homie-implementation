namespace YahiTests;

public class CommonStuff {
    public static string[] BadTopicLevels = new[] { null, "", "no/slash", "-startsWithHyphen", "UPPERcase", "bad!#$%&*()characters" };
    public static string[] GoodTopicLevels = new[] { "a-nice-id", "base", "two-words", "w1th-numb3rs", "stupid-but-----still-val1d" };
    public static string[] BadFloatValues = new[] { "12,3", "-12f.45", "000.3", "1.", ".6", "-.67", "0.", ".0", "00.0", "1.e3", "-.5E5", "12.1234e-5" };
    public static string[] GoodFloatValues = new[] { "12.4", "-10.454", "3.000", "0.0", "1.0e12", "-1.00E-12", "1.001e4" };
    public static string[] GoodIntegerValues = new[] { "1", "-1", "0", "-0", "+0", "12", "-12", "-17000000", "17000000" };
    public static string[] BadDateTimeValues = new[] { "11/11/2021 12:12:12" };
}
