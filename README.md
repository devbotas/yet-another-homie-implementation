[![NuGet](https://img.shields.io/nuget/dt/DevBot9.Protocols.Homie.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/DevBot9.Protocols.Homie/) 
[![NuGet](https://img.shields.io/nuget/vpre/DevBot9.Protocols.Homie.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/DevBot9.Protocols.Homie/) 


This is a C# implementation of [Homie Convention 4.0.0](https://homieiot.github.io/specification/). 

Homie Convention targets MQTT transport and adds an application-level protocol to standartize ways of how devices talk to each other, since MQTT doesn't really have any such facilities. Also, Homie emphasizes discoverability, so, for example, if a new device joins the network, any controller could immediatelly figure out its properties and capabilities.

# Getting started with YAHI
Apparently, you need an MQTT broker first. [Mosquitto](https://mosquitto.org/) is probably the most popular free one. It run on anything, but running it on NAS in a Docker container is probably the most convenient option.

Then, check out the ```TestApp```. It has multiple *simulated* devices, like ```AirConditioner```, which not only have some Homie properties implemented, but they also simulate real-world counterparts, so you could, for example, set the target temperature and see how it actually slowly changes. You can add these devices to ```OpenHAB```, ```HomeAssistant``` or other home automation platforms and start clicking button right away.

Of course, if developing new devices, you need some better debugging tools. To watch raw MQTT traffic, check out [MQTT explorer](https://mqtt-explorer.com/). To quickly  check the controls you created, try [HoDD](https://mqtt-explorer.com/) (although it only works if downloaded and run from the PC directly).

# Important YAHI design concepts
YAHI is fully-Homie compliant, however, it is a *slightly more strict subset*. 

## States, Commands, Parameters
YAHI defines three distinct types of properties: ```States```, ```Commands``` and ```Parameters```. Homie allows a fourth type, which I found redundant and very weakly defined, so it has not made it into YAHI. But rest assured, you don't need it at all. More on that later.

**States** have read-only access, and is something that user cannot change directly. It is usually a measured or calculated value, like, for example, an ambient temperature. Reading a state multiple times may result in different values.

**Commands** have write-only access, and, as the name suggests, sending a command invokes some action. For example, "turn on" would be a command. It is possible to send the same command multiple times, but it is up for the device whether to execute it or not (like, one can't really turn on a device when it is already on).

**Parameters** have read/write access, and are used in calculations or for defining behaviour. For example, "target temperature" of an A/C unit. Important implementation thing about parameters is that _they can only be changed by user_. Device cannot alter parameter value by itself, it has to be either directly set by a user, or via indirect command like "reset to factory defaults", which is still invoked manually. Rreading a parameter multiple time should always return last set value.

## Unsupported data types

In short: ```integer```, ```percent``` and ```boolean``` data types aren't supported by design.

```percent``` is a very confusing data type. It requires ```$unit``` field to be set to "%", and the payload can be either an ```integer``` or a ```float```. That itself creates enough confusion and parsing problems. However, one may also define percent payload using casual ```integer``` or ```float```, so we get three ways to define percentage payload:
- ```$datatype=percent```, ```$unit="%"```
- ```$datatype=float```, ```$unit="%"```
- ```$datatype=integer```, ```$unit="%"```

So because of that, support for ```percent``` data type was dropped, because the same can be achieved using casual number data types.

Support for ```boolean``` was dropped because it is just a subset of ```enum``` data type with possible values being ```true``` and ```false```. Also, usage of ```boolean``` type feels very unnatural. Homie IOT Convention is quite a humanly one, but humans do not speak in ```true``` and ```false```. If there's a question "is the light switch turned on?", nobody answers "false"; natural answers would be "no", "nope", "it is not" and similar. 

This is also problematic for frontends and GUIs. Systems like OpenHAB are generic ones; ```boolean``` data type had two predefined states negating each other, so that begs to render a simple visual switch. But switch positions must have labels of some sort, and ```boolean```data type forces to use "true" and "false", resulting in a very unnatural visual representation. However, one may hardly find such "true/false" switch in reality. It is always "on/off", "yes/no", maybe even "1/0", but not "true/false".

So for these reasons, ```boolean``` had to go.




*readme unfinished*
