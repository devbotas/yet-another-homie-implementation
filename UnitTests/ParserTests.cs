using System.Collections;
using DevBot9.Protocols.Homie;
using NUnit.Framework;

namespace YahiTests {
    class ParserTests {
        ClientPropertyMetadata _property;

        [SetUp]
        public void Setup() {
            _property = new ClientPropertyMetadata();
            _property.NodeId = "test-node";
            _property.PropertyId = "test-property";
            _property.Name = "test-name";
            _property.PropertyType = PropertyType.Parameter;
            _property.DataType = DataType.Blank;
            _property.Format = "";
            _property.Unit = "";
            _property.InitialValue = "";
        }

        [Test]
        public void ClearInitialValueForCommands() {
            _property.PropertyType = PropertyType.Command;
            _property.DataType = DataType.String;
            _property.InitialValue = "wrong-value";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsTrue(validationResult);
            Assert.IsEmpty(_property.InitialValue);
            Assert.Zero(errorList.Count);
            Assert.NotZero(warningList.Count);
        }

        [Test]
        public void CheckBadColorFormat() {
            _property.DataType = DataType.Color;
            _property.InitialValue = "0,0,0";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);


            _property.Format = "bbd";
            errorList = new ArrayList();
            warningList = new ArrayList();
            validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);
        }

        [Test]
        public void CheckBadColorInitialValue() {
            _property.DataType = DataType.Color;
            _property.Format = "rgb";
            _property.InitialValue = "256,-1,255";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);


            _property.DataType = DataType.Color;
            _property.Format = "hsv";
            _property.InitialValue = "361,1,99";
            errorList = new ArrayList();
            warningList = new ArrayList();
            validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);
        }

        [Test]
        public void CheckBadFloatInitialValue() {
            _property.DataType = DataType.Float;

            foreach (var value in CommonStuff.BadFloatValues) {
                _property.InitialValue = value;

                var errorList = new ArrayList();
                var warningList = new ArrayList();
                var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
                Assert.IsFalse(validationResult, $"Ooops, for initial value of {value} this test should fail!");
                Assert.NotZero(errorList.Count);
            }
        }

        [Test]
        public void CheckBadIntegerInitialValue() {
            _property.DataType = DataType.Integer;

            foreach (var value in CommonStuff.BadFloatValues) {
                _property.InitialValue = value;

                var errorList = new ArrayList();
                var warningList = new ArrayList();
                var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
                Assert.IsFalse(validationResult, $"Ooops, for initial value of {value} this test should fail!");
                Assert.NotZero(errorList.Count);
            }

            foreach (var value in CommonStuff.GoodFloatValues) {
                _property.InitialValue = value;

                var errorList = new ArrayList();
                var warningList = new ArrayList();
                var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
                Assert.IsFalse(validationResult, $"Ooops, for initial value of {value} this test should fail!");
                Assert.NotZero(errorList.Count);
            }
        }

        [Test]
        public void ConvertIntegerToFloat() {
            _property.DataType = DataType.Integer;
            _property.InitialValue = "1";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsTrue(validationResult);
            Assert.AreEqual(_property.DataType, DataType.Float);
        }

        [Test]
        public void CheckBadEnumFormat() {
            _property.DataType = DataType.Enum;
            _property.Format = "bad-format";
            _property.InitialValue = "bad-format";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);
        }

        [Test]
        public void CheckBadEnumInitialValue() {
            _property.DataType = DataType.Enum;
            _property.Format = "one,two";
            _property.InitialValue = "three";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsFalse(validationResult);
            Assert.NotZero(errorList.Count);
        }

        [Test]
        public void ConvertBooleanToEnum() {
            _property.DataType = DataType.Boolean;
            _property.InitialValue = "true";
            var errorList = new ArrayList();
            var warningList = new ArrayList();
            var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
            Assert.IsTrue(validationResult);
            Assert.AreEqual(_property.DataType, DataType.Enum);
            Assert.AreEqual(_property.Format, "false,true");
        }

        [Test]
        public void CheckBadDateTimeInitialValue() {
            _property.DataType = DataType.Integer;

            foreach (var value in CommonStuff.BadDateTimeValues) {
                _property.InitialValue = value;

                var errorList = new ArrayList();
                var warningList = new ArrayList();
                var validationResult = _property.ValidateAndFix(ref errorList, ref warningList);
                Assert.IsFalse(validationResult, $"Ooops, for initial value of {value} this test should fail!");
                Assert.NotZero(errorList.Count);
            }
        }
    }
}
