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
            foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
                Assert.That(() => DeviceFactory.Initialize(badTopicLevel), Throws.ArgumentException);
            }

            foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
                Assert.DoesNotThrow(() => DeviceFactory.Initialize(goodTopicLevel));
            }

            var baseTopic = "provided-topic";
            DeviceFactory.Initialize(baseTopic);
            if (DeviceFactory.BaseTopic != baseTopic) { Assert.Fail($"Base topic is not {baseTopic}"); };
        }

        [Test]
        public void CreateClientDeviceWithIncorrectId() {
            foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
                Assert.That(() => DeviceFactory.CreateClientDevice(badTopicLevel), Throws.ArgumentException);
            }

            foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
                Assert.DoesNotThrow(() => DeviceFactory.CreateClientDevice(goodTopicLevel));
            }
        }

        [Test]
        public void CreateHostDeviceWithIncorrectId() {
            foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
                Assert.That(() => DeviceFactory.CreateHostDevice(badTopicLevel, "some friendly name"), Throws.ArgumentException);
            }

            foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
                Assert.DoesNotThrow(() => DeviceFactory.CreateHostDevice(goodTopicLevel, "some friendly name"));
            }
        }
    }
}
