using System.Collections;
using System.Text.RegularExpressions;

namespace DevBot9.Protocols.Homie {
    public class HomieTopicTreeParser {
        /// <summary>
        /// Parses Homie tree from provided topic and value dump.
        /// </summary>
        /// <param name="input">Each line should follow this format: {topic}:{value}. For example, homie/lightbulb/$homie:4.0.0</param>       
        public static ClientDeviceMetadata[] Parse(string[] input, string baseTopic, out string[] problemList) {
            var tempProblemList = new ArrayList();

            // First, need to figure out ho many devices are in the input dump. Looking for $homie attributes.
            var foundDeviceIds = new ArrayList();
            var distinctDevicesRegex = new Regex($@"^({baseTopic})\/([a-z0-9][a-z0-9-]+)\/(\$homie):(\S+)$");
            foreach (var inputString in input) {
                var regexMatch = distinctDevicesRegex.Match(inputString);
                if (regexMatch.Success) {
                    foundDeviceIds.Add(regexMatch.Groups[2].Value);
                }
            }

            // Grouping topics by device, so we don't have to reiterate over full list over and over again.
            var sortedTopics = new Hashtable();
            foreach (var inputString in input) {
                for (var d = 0; d < foundDeviceIds.Count; d++) {
                    var deviceId = (string)foundDeviceIds[d];
                    // Adding a new device to hashtable, if it is not there yet.
                    if (sortedTopics.Contains(deviceId) == false) { sortedTopics.Add(deviceId, new ArrayList()); }

                    // Adding a relevant topic for that device.
                    if (inputString.StartsWith($@"{baseTopic}/{deviceId}/")) { ((ArrayList)(sortedTopics[deviceId])).Add(inputString); }
                }
            }

            // Now, iterating over devices we have just found and trying our best to parse as much as possible.
            var goodDevices = new ArrayList();
            for (var d = 0; d < foundDeviceIds.Count; d++) {
                var candidateId = (string)foundDeviceIds[d];
                var candidateTopics = (ArrayList)sortedTopics[candidateId];
                var candidateDevice = new ClientDeviceMetadata(candidateId);

                if (candidateDevice.TryParse(candidateTopics, ref tempProblemList)) { goodDevices.Add(candidateDevice); }
            }

            // Converting local temporary lists to final arrays and returning.
            problemList = new string[tempProblemList.Count];
            for (var i = 0; i < tempProblemList.Count; i++) {
                problemList[i] = (string)tempProblemList[i];
            }

            var deviceTree = new ClientDeviceMetadata[goodDevices.Count];
            for (var i = 0; i < goodDevices.Count; i++) {
                deviceTree[i] = (ClientDeviceMetadata)goodDevices[i];
            }

            return deviceTree;
        }
    }
}
