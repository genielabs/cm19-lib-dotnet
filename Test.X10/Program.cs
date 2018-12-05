/*
  This file is part of CM19Lib (https://github.com/genielabs/cm19-lib-dotnet)
 
  Copyright (2012-2018) G-Labs (https://github.com/genielabs)

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

/*
 *     Author: Generoso Martello <gene@homegenie.it>
 *     Project Homepage: https://github.com/genielabs/cm19-lib-dotnet
 */

using System;
using System.Threading;

using CM19Lib;
using CM19Lib.Events;
using CM19Lib.X10;

namespace Test.X10
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // NOTE: To disable debug output uncomment the following two lines
            //LogManager.Configuration.LoggingRules.RemoveAt(0);
            //LogManager.Configuration.Reload();

            Console.WriteLine("XTenLib test program, waiting for connection.");
            var cm19 = new X10RfManager();
            // Listen to X10RfManager events
            cm19.ConnectionStatusChanged += cm19_ConnectionStatusChanged;
            // RF events
            cm19.RfDataReceived += cm19_RfDataReceived;
            cm19.RfCommandReceived += cm19_RfCommandReceived;
            cm19.RfSecurityReceived += cm19_RfSecurityReceived;
            // TODO: cm19.RfCameraReceived += cm19_RfCameraReceived;
            // Connect to the interface
            cm19.Connect();
            
            // Examples of sending X10 commands
            //cm19.UnitOff(HouseCode.C, UnitCode.Unit_7);
            //cm19.UnitOn(HouseCode.A, UnitCode.Unit_4);
            //cm19.Dim(HouseCode.A);
            //cm19.Bright(HouseCode.A);
            //cm19.AllLightsOn(HouseCode.A);
            //cm19.AllUnitsOff(HouseCode.A);
            // Alternative way of sending X10 commands
            //cm19.SendCommand(HouseCode.E, UnitCode.Unit_12, Command.On);
            // Raw send X10 command (Security Disarm)
            //cm19.SendMessage(new byte[]{0x29, 0x66, 0x69, 0x86, 0x79, 0x4A, 0x80});

            // Prevent the program from quitting with a noop loop
            while (true)
            {
                Thread.Sleep(1000);
            }

        }

        private static void cm19_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("Interface connection status {0}", args.Connected);
        }

        private static void cm19_RfDataReceived(object sender, RfDataReceivedEventArgs args)
        {
            Console.WriteLine("RF data received: {0}", BitConverter.ToString(args.Data));
        }

        private static void cm19_RfCommandReceived(object sender, RfCommandReceivedEventArgs args)
        {
            Console.WriteLine("Received RF command {0} House Code {1} Unit {2}", args.Command, args.HouseCode, args.UnitCode.ToString().Replace("Unit_", ""));
        }

        private static void cm19_RfSecurityReceived(object sender, RfSecurityReceivedEventArgs args)
        {
            Console.WriteLine("Received RF Security event {0} from address {1}", args.Event, args.Address.ToString("X3"));
        }
    }
}
