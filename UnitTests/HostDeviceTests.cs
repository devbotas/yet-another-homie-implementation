using System;
using DevBot9.Protocols.Homie;
using NUnit.Framework;

namespace YahiTests;

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

    [Test]
    public void CreateHostFloatPropertyValidateNodeIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, badTopicLevel, "good-id", "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, goodTopicLevel, "good-id", "Friendly name"));
        }
    }

    [Test]
    public void CreateHostFloatPropertyValidatePropertyIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "good-id", badTopicLevel, "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "good-id", goodTopicLevel, "Friendly name"));
        }
    }

    [Test]
    public void CreateHostStringPropertyValidateNodeIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostTextProperty(PropertyType.Parameter, badTopicLevel, "good-id", "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostTextProperty(PropertyType.Parameter, goodTopicLevel, "good-id", "Friendly name"));
        }
    }

    [Test]
    public void CreateHostStringPropertyValidatePropertyIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostTextProperty(PropertyType.Parameter, "good-id", badTopicLevel, "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostTextProperty(PropertyType.Parameter, "good-id", goodTopicLevel, "Friendly name"));
        }
    }

    [Test]
    public void CreateHostColorPropertyValidateNodeIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostColorProperty(PropertyType.Parameter, badTopicLevel, "good-id", "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, goodTopicLevel, "good-id", "Friendly name"));
        }
    }

    [Test]
    public void CreateHostColorPropertyValidatePropertyIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "good-id", badTopicLevel, "Friendly name"), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "good-id", goodTopicLevel, "Friendly name"));
        }
    }

    [Test]
    public void CreateHostEnumPropertyValidateNodeIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, badTopicLevel, "good-id", "Friendly name", new[] { "One", "Two" }), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, goodTopicLevel, "good-id", "Friendly name", new[] { "One", "Two" }));
        }
    }

    [Test]
    public void CreateHostEnumPropertyValidatePropertyIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, "good-id", badTopicLevel, "Friendly name", new[] { "One", "Two" }), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, "good-id", goodTopicLevel, "Friendly name", new[] { "One", "Two" }));
        }
    }

    [Test]
    public void CreateHostDateTimePropertyValidateNodeIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostDateTimeProperty(PropertyType.Parameter, badTopicLevel, "good-id", "Friendly name", DateTime.Now), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostDateTimeProperty(PropertyType.Parameter, goodTopicLevel, "good-id", "Friendly name", DateTime.Now));
        }
    }

    [Test]
    public void CreateHostDateTimePropertyValidatePropertyIdInput() {
        foreach (var badTopicLevel in CommonStuff.BadTopicLevels) {
            Assert.That(() => _hostDevice.CreateHostDateTimeProperty(PropertyType.Parameter, "good-id", badTopicLevel, "Friendly name", DateTime.Now), Throws.ArgumentException);
        }

        foreach (var goodTopicLevel in CommonStuff.GoodTopicLevels) {
            Assert.DoesNotThrow(() => _hostDevice.CreateHostDateTimeProperty(PropertyType.Parameter, "good-id", goodTopicLevel, "Friendly name", DateTime.Now));
        }
    }
}
