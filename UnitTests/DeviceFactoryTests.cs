using DevBot9.Protocols.Homie;
using NUnit.Framework;

namespace YahiTests {
    public class DeviceFactoryTests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void InitializeWithDefaultTopic() {
            DeviceFactory.Initialize();

            if (DeviceFactory.BaseTopic != "homie") { Assert.Fail("Base topic is not \"homie\""); };
        }

        [Test]
        public void InitializeWithProvidedBaseTopic() {
            var baseTopic = "provided-topic";

            DeviceFactory.Initialize(baseTopic);

            if (DeviceFactory.BaseTopic != baseTopic) { Assert.Fail($"Base topic is not {baseTopic}"); };
        }

        [Test]
        public void CreateClientDeviceWithIncorrectId() {
            Assert.That(() => DeviceFactory.CreateClientDevice(null), Throws.ArgumentException);
            Assert.That(() => DeviceFactory.CreateClientDevice(""), Throws.ArgumentException);
            Assert.That(() => DeviceFactory.CreateClientDevice("no/slash"), Throws.ArgumentException);
        }

        [Test]
        public void CreateHostDeviceWithIncorrectId() {
            Assert.That(() => DeviceFactory.CreateHostDevice(null, "some friendly name"), Throws.ArgumentException);
            Assert.That(() => DeviceFactory.CreateHostDevice("", "some friendly name"), Throws.ArgumentException);
            Assert.That(() => DeviceFactory.CreateHostDevice("no/slash", "some friendly name"), Throws.ArgumentException);
        }
    }
}
