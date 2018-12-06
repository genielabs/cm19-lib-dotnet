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

namespace cm19recv
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("CM19 RF receiver program (press CTRL+C to exit).");
            var cm19 = new Cm19Manager();
            // Listen to Cm19Manager events
            cm19.ConnectionStatusChanged += cm19_ConnectionStatusChanged;
            // RF events
            cm19.RfDataReceived += cm19_RfDataReceived;
            cm19.RfCommandReceived += cm19_RfCommandReceived;
            cm19.RfSecurityReceived += cm19_RfSecurityReceived;
            // TODO: cm19.RfCameraReceived += cm19_RfCameraReceived;
            // Connect to the interface
            cm19.Connect();

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
