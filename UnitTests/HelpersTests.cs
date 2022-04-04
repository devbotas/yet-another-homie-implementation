using DevBot9.Protocols.Homie;
using NUnit.Framework;

namespace YahiTests;

class HelpersTests {

    [SetUp]
    public void Setup() {

    }

    [Test]
    public void TestIsInteger() {
        foreach (var value in CommonStuff.BadFloatValues) {
            var result = Helpers.IsInteger(value);

            Assert.IsFalse(result, $"Ooops, for initial value of {value} this test should fail!");
        }

        foreach (var value in CommonStuff.GoodFloatValues) {
            var result = Helpers.IsInteger(value);

            Assert.IsFalse(result, $"Ooops, for initial value of {value} this test should fail!");
        }

        foreach (var value in CommonStuff.GoodIntegerValues) {
            var result = Helpers.IsInteger(value);

            Assert.IsTrue(result, $"Ooops, for initial value of {value} this test should pass!");
        }
    }

    [Test]
    public void TestIsFloat() {
        foreach (var value in CommonStuff.BadFloatValues) {
            var result = Helpers.IsFloat(value);

            Assert.IsFalse(result, $"Ooops, for initial value of {value} this test should fail!");
        }

        foreach (var value in CommonStuff.GoodFloatValues) {
            var result = Helpers.IsFloat(value);

            Assert.IsTrue(result, $"Ooops, for initial value of {value} this test should pass!");
        }

        foreach (var value in CommonStuff.GoodIntegerValues) {
            var result = Helpers.IsFloat(value);

            Assert.IsTrue(result, $"Ooops, for initial value of {value} this test should pass!");
        }
    }
}
