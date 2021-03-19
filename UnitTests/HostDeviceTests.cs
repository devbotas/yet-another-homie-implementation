using DevBot9.Protocols.Homie;
using NUnit.Framework;

namespace YahiTests {
    public class HostDeviceTests {
        private HostDevice _hostDevice;

        [SetUp]
        public void Setup() {
            DeviceFactory.Initialize();

            _hostDevice = DeviceFactory.CreateHostDevice("test-device", "");
        }

        [Test]
        public void UpdateNodeInfoValidateInput() {
            foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
                Assert.That(() => _hostDevice.UpdateNodeInfo(badTopicLevel, "whatever", "whatever"), Throws.ArgumentException);
            }

            foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
                Assert.DoesNotThrow(() => _hostDevice.UpdateNodeInfo(goodTopicLevel, "whatever", "whatever"));
            }
        }
    }
}
