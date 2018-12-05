[![Build status](https://ci.appveyor.com/api/projects/status/tpg5mnp8a4j2ehgg?svg=true)](https://ci.appveyor.com/project/genemars/cm19-lib-dotnet)
[![NuGet](https://img.shields.io/nuget/v/CM19Lib.svg)](https://www.nuget.org/packages/CM19Lib/)
![License](https://img.shields.io/github/license/genielabs/cm19-lib-dotnet.svg)

# CM19 X10 RF transceiver library for Home Automation (.NET)

## Features

- Event driven
- Hot plug
- Automatically restabilish connection on error/disconnect
- Compatible with Mono

## Requirements for using with CM19 interface

### Linux / Mac OSX

Install the libusb-1.0 package

    apt-get install libusb-1.0-0 libusb-1.0-0-dev

### Windows

Install the CM19 LibUSB driver by executing the **InstallDriver.exe** file contained in the **WindowsUsbDriver** folder.

## NuGet Package

CM19Lib is available as a [NuGet package](https://www.nuget.org/packages/CM19Lib).

Run `Install-Package CM19Lib` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) or search for “CM19Lib” in your IDE’s package management plug-in.

## Example usage

```csharp
using CM19Lib;

//...

var cm19 = new Cm19Manager();

// Listen to Cm19Manager events
cm19.ConnectionStatusChanged += cm19_ConnectionStatusChanged;
cm19.RfDataReceived += cm19_RfDataReceived;
cm19.RfCommandReceived += cm19_RfCommandReceived;
cm19.RfSecurityReceived += cm19_RfSecurityReceived;

//...

// Connect the interface. It supports hot plug/unplug.
cm19.Connect();

//...
            
// Examples of sending X10 commands
cm19.UnitOff(HouseCode.C, UnitCode.Unit_7);
cm19.UnitOn(HouseCode.A, UnitCode.Unit_4);
cm19.Dim(HouseCode.A);
cm19.Bright(HouseCode.A);
cm19.AllLightsOn(HouseCode.A);
cm19.AllUnitsOff(HouseCode.A);
// Alternative way of sending X10 commands
cm19.SendCommand(HouseCode.E, UnitCode.Unit_12, Command.On);
// Raw send X10 command (Security Disarm)
cm19.SendMessage(new byte[]{0x29, 0x66, 0x69, 0x86, 0x79, 0x4A, 0x80});

//...

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
```

## License

CM19Lib is open source software, licensed under the terms of Apache license 2.0. See the [LICENSE](LICENSE) file for details.
