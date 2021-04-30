[![NuGet](https://img.shields.io/nuget/dt/DevBot9.Protocols.Homie.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/DevBot9.Protocols.Homie/) 

This is a C# implementation of [Homie Convention 4.0.0](https://homieiot.github.io/specification/). 

Homie Convention targets MQTT transport and adds an application-level protocol to standartize ways of how devices talk to each other, since MQTT doesn't really have any such facilities. Also, Homie emphasizes discoverability, so, for example, if a new device joins the network, any controller could immediatelly figure out its properties and capabilities.

# Getting started with YAHI
Apparently, you need an MQTT broker first. [Mosquitto](https://mosquitto.org/) is probably the most popular free one. It run on anything, but running it on NAS in a Docker container is probably the most convenient option.

Then, check out the ```TestApp```. It has multiple *simulated* devices, like ```AirConditioner```, which not only have some Homie properties implemented, but they also simulate real-world counterparts, so you could, for example, set the target temperature and see how it actually slowly changes. You can add these devices to ```OpenHAB```, ```HomeAssistant``` or other home automation platforms and start clicking button right away.

Of course, if developing new devices, you need some better debugging tools. To watch raw MQTT traffic, check out [MQTT explorer](https://mqtt-explorer.com/). To quickly  check the controls you created, try [HoDD](https://mqtt-explorer.com/) (although it only works if downloaded and run from the PC directly).
# Important YAHI design concepts
YAHI is fully-Homie compliant, however, it is a *slightly more strict subset*. YAHI defines three distinct types of properties: ```States```, ```Commands``` and ```Parameters```. Homie allows a fourth type, which I found redundant and very weakly defined, so it has not made it into YAHI. But rest assured, you don't need it at all. More on that later.

## State
A state is something that you cannot directly change. It is usually something that is measured, like, for example, an ambient temperature.

## Command

## Parameter

*readme unfinished*
