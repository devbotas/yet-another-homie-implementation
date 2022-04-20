using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DevBot9.Protocols.Homie;

/// <summary>
/// This class is useful when parsing MQTT topics trying to figure out if those topics are actually correct. If they are, then this class is later used to create a <see cref="ClientDevice"/>.
/// </summary>
public class ClientDeviceMetadata {
    public string Id { get; internal set; } = "";
    public string HomieAttribute { get; internal set; } = "";
    public string NameAttribute { get; internal set; } = "";
    public string StateAttribute { get; internal set; } = "";
    public ClientNodeMetadata[] Nodes { get; internal set; }
    public Dictionary<string, string> AllAttributes { get; internal set; } = new();

    internal ClientDeviceMetadata() {
        // Making the constructor inaccessible for public use.
    }

    /// <summary>
    /// Tries parsing a whole tree of a single device.
    /// </summary>
    public static bool TryParse(List<string> topicList, string baseTopic, string deviceId, out ClientDeviceMetadata parsedClientDeviceMetadata, ref List<string> errorList, ref List<string> warningList) {
        var isParsedWell = false;
        var candidateDevice = new ClientDeviceMetadata() { Id = deviceId };
        var parsedTopicList = new List<string>();

        // It will be reassigned on successful parsing.
        parsedClientDeviceMetadata = null;

        var attributesGood = TryParseAttributes(ref topicList, ref parsedTopicList, baseTopic, ref candidateDevice, ref errorList, ref warningList);
        if (attributesGood) {
            var nodesGood = TryParseNodes(ref topicList, ref parsedTopicList, baseTopic, ref candidateDevice, ref errorList, ref warningList);
            if (nodesGood) {
                var propertiesGood = TryParseProperties(ref topicList, ref parsedTopicList, baseTopic, ref candidateDevice, ref errorList, ref warningList);
                if (propertiesGood) {
                    TrimRottenBranches(ref candidateDevice, ref errorList, ref warningList);
                    isParsedWell = true;
                    parsedClientDeviceMetadata = candidateDevice;
                }
                else {
                    errorList.Add($"Device '{candidateDevice.Id}' was detected, but it does not have a single valid property. This device subtree will be skipped.");
                }
            }
            else {
                errorList.Add($"Device '{candidateDevice.Id}' was detected, but it does not have a single valid node. This device subtree will be skipped.");
            }
        }
        else {
            errorList.Add($"Device '{candidateDevice.Id}' was detected, but it is missing important attributes. This device subtree will be skipped.");
        }

        return isParsedWell;
    }

    private static bool TryParseAttributes(ref List<string> unparsedTopicList, ref List<string> parsedTopicList, string baseTopic, ref ClientDeviceMetadata candidateDevice, ref List<string> errorList, ref List<string> warningList) {
        // Filtering out device attributes. We'll get nodes from them.
        var deviceAttributesRegex = new Regex($@"^({baseTopic})\/({candidateDevice.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
        var isHomieReceived = false;
        var isDeviceNameReceived = false;
        var isNodesReceived = false;
        var isStateReceived = false;

        foreach (var inputString in unparsedTopicList) {
            var regexMatch = deviceAttributesRegex.Match(inputString);
            if (regexMatch.Success) {
                var key = regexMatch.Groups[3].Value;
                var value = regexMatch.Groups[4].Value;

                if (key == "$homie") {
                    isHomieReceived = true;
                    candidateDevice.HomieAttribute = value;
                }
                if (key == "$name") {
                    isDeviceNameReceived = true;
                    candidateDevice.NameAttribute = value;
                }
                if (key == "$state") {
                    isStateReceived = true;
                    candidateDevice.StateAttribute = value;
                }
                if (key == "$nodes") {
                    isNodesReceived = true;
                }

                candidateDevice.AllAttributes.Add(key, value);
                parsedTopicList.Add(inputString);
            }
        }

        foreach (var parsedTopic in parsedTopicList) {
            unparsedTopicList.Remove(parsedTopic);
        }

        var minimumDeviceSetReceived = isHomieReceived & isDeviceNameReceived & isNodesReceived & isStateReceived;

        bool isParseSuccessful;
        if (minimumDeviceSetReceived) {
            isParseSuccessful = true;
        }
        else {
            isParseSuccessful = false;
            if (isHomieReceived == false) { errorList.Add($"Device '{candidateDevice.Id}': mandatory attribute $homie was not found, parsing cannot continue."); }
            if (isDeviceNameReceived == false) { errorList.Add($"Device '{candidateDevice.Id}': mandatory attribute $name was not found, parsing cannot continue."); }
            if (isNodesReceived == false) { errorList.Add($"Device '{candidateDevice.Id}': mandatory attribute $nodes was not found, parsing cannot continue."); }
            if (isStateReceived == false) { errorList.Add($"Device '{candidateDevice.Id}': mandatory attribute $state was not found, parsing cannot continue."); }
        }

        return isParseSuccessful;
    }
    private static bool TryParseNodes(ref List<string> unparsedTopicList, ref List<string> parsedTopicList, string baseTopic, ref ClientDeviceMetadata candidateDevice, ref List<string> errorList, ref List<string> warningList) {
        var isParseSuccessful = true;

        var candidateNodeIds = ((string)candidateDevice.AllAttributes["$nodes"]).Split(',');
        var goodNodes = new List<ClientNodeMetadata>();

        for (var n = 0; n < candidateNodeIds.Length; n++) {
            var candidateNode = new ClientNodeMetadata() { Id = candidateNodeIds[n] };

            // Filtering out attributes for this node. We'll get properties from them.
            var nodeAttributesRegex = new Regex($@"^({baseTopic})\/({candidateDevice.Id})\/({candidateNode.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
            foreach (var inputString in unparsedTopicList) {
                var regexMatch = nodeAttributesRegex.Match(inputString);
                if (regexMatch.Success) {
                    var key = regexMatch.Groups[4].Value;
                    var value = regexMatch.Groups[5].Value;

                    if (key == "$name") {
                        candidateNode.NameAttribute = value;
                    }
                    if (key == "$type") {
                        candidateNode.TypeAttribute = value;
                    }

                    candidateNode.AllAttributes.Add(regexMatch.Groups[4].Value, regexMatch.Groups[5].Value);
                    parsedTopicList.Add(inputString);
                }
            }

            foreach (var parsedTopic in parsedTopicList) {
                unparsedTopicList.Remove(parsedTopic);
            }

            // Figuring out properties we have for this node.
            if (candidateNode.AllAttributes.ContainsKey("$properties")) {
                goodNodes.Add(candidateNode);
            }
            else {
                // Something is wrong, an essential topic is missing.
                errorList.Add($"{candidateDevice.Id}/{candidateNode.Id} is defined, but $properties attribute is missing. This node subtree will be skipped entirely.");
            }
        }

        // Should be at least one valid node. If not - problem.
        if (goodNodes.Count == 0) { isParseSuccessful = false; }

        // Converting local temporary lists to final arrays and returning.
        candidateDevice.Nodes = new ClientNodeMetadata[goodNodes.Count];
        for (var i = 0; i < goodNodes.Count; i++) {
            candidateDevice.Nodes[i] = goodNodes[i];
        }

        return isParseSuccessful;
    }
    private static bool TryParseProperties(ref List<string> unparsedTopicList, ref List<string> parsedTopicList, string baseTopic, ref ClientDeviceMetadata candidateDevice, ref List<string> errorList, ref List<string> warningList) {
        var isParseSuccessful = false;

        for (var n = 0; n < candidateDevice.Nodes.Length; n++) {
            var candidatePropertyIds = ((string)candidateDevice.Nodes[n].AllAttributes["$properties"]).Split(',');
            var goodProperties = new List<ClientPropertyMetadata>();

            for (var p = 0; p < candidatePropertyIds.Length; p++) {
                var candidateProperty = new ClientPropertyMetadata() { NodeId = candidateDevice.Nodes[n].Id, PropertyId = candidatePropertyIds[p] };

                // Parsing property attributes and value.
                var attributeRegex = new Regex($@"^({baseTopic})\/({candidateDevice.Id})\/({candidateDevice.Nodes[n].Id})\/({candidateProperty.PropertyId})(\/\$[a-z0-9][a-z0-9-]+)?(:)(.+)$");
                var setRegex = new Regex($@"^({baseTopic})\/({candidateDevice.Id})\/({candidateDevice.Nodes[n].Id})\/({candidateProperty.PropertyId})(\/set)(:)(.+)$");
                var valueRegex = new Regex($@"^({baseTopic})\/({candidateDevice.Id})\/({candidateDevice.Nodes[n].Id})\/({candidateProperty.PropertyId})()(:)(.+)$");

                var isSettable = false;
                var isRetained = false;
                var isNameReceived = false;
                var isDataTypeReceived = false;
                var isSettableReceived = false;
                var isRetainedReceived = false;

                foreach (var inputString in unparsedTopicList) {
                    var attributeMatch = attributeRegex.Match(inputString);
                    if (attributeMatch.Success) {
                        var key = attributeMatch.Groups[5].Value;
                        var value = attributeMatch.Groups[7].Value;

                        if (key == "/$name") {
                            candidateProperty.Name = value;
                            isNameReceived = true;
                        }
                        if (key == "/$datatype") {
                            if (Helpers.TryParseHomieDataType(value, out var parsedType)) {
                                candidateProperty.DataType = parsedType;
                                isDataTypeReceived = true;
                            };
                        }
                        if (key == "/$format") {
                            candidateProperty.Format = value;
                        }
                        if (key == "/$settable") {
                            if (Helpers.TryParseBool(value, out isSettable)) {
                                isSettableReceived = true;
                            }
                        }
                        if (key == "/$retained") {
                            if (Helpers.TryParseBool(value, out isRetained)) {
                                isRetainedReceived = true;
                            };
                        }

                        if (key == "/$unit") {
                            candidateProperty.Unit = value;
                        }

                        parsedTopicList.Add(inputString);
                    }

                    var setMatch = setRegex.Match(inputString);
                    if (setMatch.Success) {
                        // Discarding this one. This is a historically cached command, and these should not execute during initialization. Besides, it shouldn't be retained,
                        // so the fact that we're here means something is wrong with the host side.
                        warningList.Add($"{candidateDevice.Id}/{candidateProperty} has /set topic assigned, which means /set message is published as retained. This is against Homie convention.");
                    }

                    var valueMatch = valueRegex.Match(inputString);
                    if (valueMatch.Success) {
                        var value = attributeMatch.Groups[7].Value;
                        candidateProperty.InitialValue = value;
                    }
                }

                foreach (var parsedTopic in parsedTopicList) {
                    unparsedTopicList.Remove(parsedTopic);
                }

                // Basic data extraction is done. Now we'll validate if values of the fields are compatible with each other and also YAHI itself.
                var isOk = isNameReceived & isDataTypeReceived & isSettableReceived & isRetainedReceived;
                if (isOk) {
                    // Setting property type.
                    if ((isSettable == false) && (isRetained == true)) {
                        candidateProperty.PropertyType = PropertyType.State;
                    }
                    if ((isSettable == true) && (isRetained == false)) {
                        candidateProperty.PropertyType = PropertyType.Command;
                    }
                    if ((isSettable == true) && (isRetained == true)) {
                        candidateProperty.PropertyType = PropertyType.Parameter;
                    }
                    if ((isSettable == false) && (isRetained == false)) {
                        errorList.Add($"{candidateDevice.Id}/{candidateProperty.NodeId}/{candidateProperty.PropertyId} has all mandatory fields set, but this retainability and settability configuration is not supported by YAHI. Skipping this property entirely.");
                        isOk = false;
                    }

                }
                else {
                    // Some of the mandatory topic were not received. Can't let this property through.
                    errorList.Add($"{candidateDevice.Id}/{candidateProperty.NodeId}/{candidateProperty.PropertyId} is defined, but mandatory attributes are missing. Skipping this property entirely.");
                    isOk = false;
                }

                // Validating by property data type, because rules are very different for each of those.
                if (isOk) {
                    var tempErrorList = new List<string>();
                    var tempWarningList = new List<string>();

                    // ValidateAndFix method does not know anythin about device it is parsing, thus doing some wrapping around error and warning lists (because I want device info in those).
                    isOk = candidateProperty.ValidateAndFix(ref tempErrorList, ref tempWarningList);

                    foreach (var error in tempErrorList) {
                        errorList.Add($"{candidateDevice.Id}/{error}");
                    }
                    foreach (var warning in tempWarningList) {
                        warningList.Add($"{candidateDevice.Id}/{warning}");
                    }
                }

                if (isOk) {
                    goodProperties.Add(candidateProperty);
                }

            }

            // Converting local temporary property lists to final arrays.
            candidateDevice.Nodes[n].Properties = new ClientPropertyMetadata[goodProperties.Count];
            for (var i = 0; i < goodProperties.Count; i++) {
                candidateDevice.Nodes[n].Properties[i] = goodProperties[i];
            }
        }

        // Converting local temporary node lists to final arrays and returning.
        foreach (var node in candidateDevice.Nodes) {
            if (node.Properties.Length > 0) {
                isParseSuccessful = true;
            }
        }

        return isParseSuccessful;
    }
    private static void TrimRottenBranches(ref ClientDeviceMetadata candidateDevice, ref List<string> problemList, ref List<string> warningList) {
        var goodNodes = new List<ClientNodeMetadata>();
        var newNodesValue = "";
        var updateNeeded = false;

        // Nodes have $properties attribute, where all the properties are listed. This list must be synchronized with actual properties.
        // The count may differ because some properties may have been rejected if there's some crucial data missing.
        foreach (var node in candidateDevice.Nodes) {
            if (node.Properties.Length > 0) {
                goodNodes.Add(node);

                var shouldBeThatManyProperties = ((string)node.AllAttributes["$properties"]).Split(',').Length;
                if (node.Properties.Length != shouldBeThatManyProperties) {
                    var newAttributeValue = node.Properties[0].PropertyId;
                    for (var i = 1; i < node.Properties.Length; i++) {
                        newAttributeValue += "," + node.Properties[i].PropertyId;
                    }
                    node.AllAttributes["$properties"] = newAttributeValue;
                    updateNeeded = true;
                }
            }
            else {
                updateNeeded = true;
            }
        }

        // Same with device, it has $nodes synced with actual node count.
        if (goodNodes.Count > 0) {
            var shouldBeThatManyNodes = ((string)candidateDevice.AllAttributes["$nodes"]).Split(',').Length;
            if (goodNodes.Count != shouldBeThatManyNodes) {
                newNodesValue = goodNodes[0].Id;
                for (var i = 1; i < goodNodes.Count; i++) {
                    newNodesValue += "," + goodNodes[i].Id;
                }
                updateNeeded = true;
            }
        }
        else {
            throw new InvalidOperationException("There should be at least a single good property at this point. Something is internally wrong with parser.");
        }

        // If needed, trimming the actual structures.
        if (updateNeeded) {
            candidateDevice.AllAttributes["$nodes"] = newNodesValue;
            candidateDevice.Nodes = new ClientNodeMetadata[goodNodes.Count];
            for (var i = 0; i < goodNodes.Count; i++) {
                candidateDevice.Nodes[i] = goodNodes[i];
            }

            warningList.Add($"Device {candidateDevice.Id} has been trimmed. Values of the fields will not be identical to what's on the broker topics!");
        }
    }

    public override string ToString() {
        return Id;
    }
}
