using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DevBot9.Protocols.Homie;

public class HomieTopicTreeParser {
    /// <summary>
    /// Parses Homie tree from provided topic and value dump.
    /// </summary>
    /// <param name="input">Each line should follow this format: {topic}:{value}. For example, homie/lightbulb/$homie:4.0.0</param>       
    public static ClientDeviceMetadata[] Parse(string[] input, string baseTopic, out string[] errorList, out string[] warningList) {
        var tempErrorList = new List<string>();
        var tempWarningList = new List<string>();

        // First, need to figure out ho many devices are in the input dump. Looking for $homie attributes.
        var foundDeviceIds = new List<string>();
        var distinctDevicesRegex = new Regex($@"^({baseTopic})\/([a-z0-9][a-z0-9-]+)\/(\$homie):(\S+)$");
        foreach (var inputString in input) {
            var regexMatch = distinctDevicesRegex.Match(inputString);
            if (regexMatch.Success) {
                foundDeviceIds.Add(regexMatch.Groups[2].Value);
            }
        }

        // Grouping topics by device, so we don't have to reiterate over full list over and over again.
        var sortedTopics = new Dictionary<string, List<string>>();
        foreach (var inputString in input) {
            for (var d = 0; d < foundDeviceIds.Count; d++) {
                var deviceId = foundDeviceIds[d];
                // Adding a new device to hashtable, if it is not there yet.
                if (sortedTopics.ContainsKey(deviceId) == false) {
                    sortedTopics.Add(deviceId, new List<string>());
                }

                // Adding a relevant topic for that device.
                if (inputString.StartsWith($@"{baseTopic}/{deviceId}/")) {
                    sortedTopics[deviceId].Add(inputString);
                }
            }
        }

        // Now, iterating over devices we have just found and trying our best to parse as much as possible.
        var goodDevices = new List<ClientDeviceMetadata>();
        for (var d = 0; d < foundDeviceIds.Count; d++) {
            var candidateId = foundDeviceIds[d];
            var candidateTopics = sortedTopics[candidateId];

            if (ClientDeviceMetadata.TryParse(candidateTopics, baseTopic, candidateId, out var candidateDevice, ref tempErrorList, ref tempWarningList)) {
                goodDevices.Add(candidateDevice);
            }
        }

        // Converting local temporary lists to final arrays and returning.
        errorList = new string[tempErrorList.Count];
        for (var i = 0; i < tempErrorList.Count; i++) {
            errorList[i] = tempErrorList[i];
        }

        warningList = new string[tempWarningList.Count];
        for (var i = 0; i < tempWarningList.Count; i++) {
            warningList[i] = tempWarningList[i];
        }

        var deviceTree = new ClientDeviceMetadata[goodDevices.Count];
        for (var i = 0; i < goodDevices.Count; i++) {
            deviceTree[i] = goodDevices[i];
        }

        return deviceTree;
    }
}
