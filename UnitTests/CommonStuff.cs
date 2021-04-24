namespace YahiTests {
    public class CommonStuff {
        public static string[] BadTopicLevels = new[] { null, "", "no/slash", "-startsWithHyphen", "UPPERcase", "bad!#$%&*()characters" };
        public static string[] GoodTopicLevels = new[] { "a-nice-id", "base", "two-words", "w1th-numb3rs", "stupid-but-----still-val1d" };
    }
}
