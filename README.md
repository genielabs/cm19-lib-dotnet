[![Build status](https://ci.appveyor.com/api/projects/status/tpg5mnp8a4j2ehgg?svg=true)](https://ci.appveyor.com/project/genemars/cm19-lib-dotnet)
[![NuGet](https://img.shields.io/nuget/v/CM19Lib.svg)](https://www.nuget.org/packages/CM19Lib/)
![License](https://img.shields.io/github/license/genielabs/cm19-lib-dotnet.svg)

# CM19 X10 RF transceiver library for Home Automation (.NET)

## Features

- Supports sending and receiving of following X10 RF messages:
    - **Standard** module commands (On, Off, Dim, Bright, AllLightsOn, AllUnitsOff)
    - **Camera** pan/tilt commands (Left, Right, Up, Down)
    - **Security** sensors (Motion and door/window sensors, security remotes)
- Event driven
- Hot plug and automatic re-connection on error

## Prerequisites

### Linux / Mac OSX

Install the libusb-1.0 package

    apt-get install libusb-1.0-0 libusb-1.0-0-dev
    
Add the following lines to the **/etc/modprobe.d/blacklist.conf** file:
```
# Disable CM19 kernel modules to allow third-party apps to access the device
blacklist lirc_atiusb
blacklist ati_remote
blacklist rc_ati_x10
```
Reboot or alternatively issue the following commands
```bash
rmmod lirc_atiusb
rmmod ati_remote
rmmod rc_ati_x10
```

### Windows

**Do not plug in the device before installing the CM19 driver**. If a driver was already installed, uninstall it first.
Then install the CM19 LibUSB driver by executing the **InstallDriver.exe** file contained in the **WindowsUsbDriver** folder.
The CM19 can now be plugged in.

## NuGet Package

CM19Lib is available as a [NuGet package](https://www.nuget.org/packages/CM19Lib).

Run `Install-Package CM19Lib` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) or search for “CM19Lib” in your IDE’s package management plug-in.

## Examples

Two examples programs are available in the *Examples* folder.
Both programs require *administrator* privileges in order to access the USB hardware.

### Receiver program

The **cm19recv** program will receive and output all X10 RF commands in a human readable form.

Example usage:
```bash
cm19recv.exe
```

### Sender program

**cm19send** is a program to send X10 command.
Example usage:
```bash
# X10 standard commands
# and camera commands (the last two)
cm19send.exe A1+ A5+ A7- A- A- C2+ AU BD
```
The program can send one ore more X10 commands (separated by a white space).
An X10 standard command starts with the house code followed by the unit code and
the **+** (**ON**) or **-** (**OFF**) symbol.

If no unit code is provided the **+** will perform a **BRIGHT** command or a **DIM**
command in case the **-** symbol is used (eg. *A+* or *A-*).

An X10 PT camera command starts with the house code followed by one of the following:
 **U** for up, **L** for left, **D** for down and **R** for right.

## Example code

```csharp
using CM19Lib;

//...

var cm19 = new Cm19Manager();

// Listen to Cm19Manager events
cm19.ConnectionStatusChanged += cm19_ConnectionStatusChanged;
cm19.RfDataReceived += cm19_RfDataReceived;
cm19.RfCommandReceived += cm19_RfCommandReceived;
cm19.RfSecurityReceived += cm19_RfSecurityReceived;
cm19.RfCameraReceived += cm19_RfCameraReceived;

//...

// Connect the interface. It supports hot plug/unplug.
cm19.Connect();

//...
            
// Examples of sending standard X10 commands
cm19.UnitOff(HouseCode.C, UnitCode.Unit_7);
cm19.UnitOn(HouseCode.A, UnitCode.Unit_4);
cm19.Dim(HouseCode.A);
cm19.Bright(HouseCode.A);
cm19.AllLightsOn(HouseCode.A);
cm19.AllUnitsOff(HouseCode.A);

// Alternative way of sending standard X10 commands
cm19.SendCommand(HouseCode.E, UnitCode.Unit_12, Command.On);

// Sending PT camera commands
cm19.SendCameraCommand(HouseCode.A, Command.CameraDown);
cm19.SendCameraCommand(HouseCode.A, Command.CameraLeft);

// Raw send X10 command (Security Disarm)
cm19.SendMessage(new byte[]{0x29, 0x66, 0x69, 0x86, 0x79, 0x4A, 0x80});

// Disconnect the interface
cm19.Disconnect();

//...

void Cm19_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
{
    Console.WriteLine("Interface connection status {0}", args.Connected);
}

void Cm19_RfDataReceived(object sender, RfDataReceivedEventArgs args)
{
    Console.WriteLine("RF data received: {0}", BitConverter.ToString(args.Data));
}

void Cm19_RfCommandReceived(object sender, RfCommandReceivedEventArgs args)
{
    Console.WriteLine("Received RF command {0} House Code {1} Unit {2}", 
        args.Command, args.HouseCode, args.UnitCode);
}

void Cm19_RfSecurityReceived(object sender, RfSecurityReceivedEventArgs args)
{
    Console.WriteLine("Received RF Security event {0} from address {1}", 
        args.Event, args.Address.ToString("X3"));
}

private static void cm19_RfCameraReceived(object sender, RfCommandReceivedEventArgs args)
{
    Console.WriteLine("Received RF camera command {0} House Code {1} Unit {2}",
        args.Command, args.HouseCode, args.UnitCode.ToString().Replace("Unit_", ""));
}
```

## License

CM19Lib is open source software, licensed under the terms of Apache license 2.0. See the [LICENSE](LICENSE) file for details.
