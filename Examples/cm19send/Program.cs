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

namespace cm19send
{
    internal class Program
    {
        private static string[] commands;
        
        public static void Main(string[] args)
        {
            Console.WriteLine("CM19 RF sender program.");
            commands = args;
            if (commands.Length == 0)
            {
                Console.WriteLine("Usage: mono cm19send.exe <command_1> [<command_2>...<command_n>]\n");
                Console.WriteLine("Example of sending standard commands:\n");
                Console.WriteLine("    mono cm19send.exe A1+ A2+ A3- A- A+\n");
                Console.WriteLine("  Will turn ON A1 and A2, OFF A3 and then it will");
                Console.WriteLine("  send a DIM and BRIGHT command (to house code A).\n");
                Console.WriteLine("Example of sending PTZ camera commands:\n");
                Console.WriteLine("    mono cm19send.exe AU AL BD BR\n");
                Console.WriteLine("  Will move camera with house code A UP and LEFT");
                Console.WriteLine("  and the camera with house code B DOWN and RIGHT.\n");
                return;
            }
            var cm19 = new Cm19Manager();
            cm19.ConnectionStatusChanged += cm19_ConnectionStatusChanged;
            cm19.Connect();            
        }

        private static void cm19_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            if (args.Connected)
            {
                sendCommands(sender as Cm19Manager);
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Error connecting to CM19.");                
                Environment.Exit(1);
            }
        }

        private static void sendCommands(Cm19Manager cm19)
        {
            HouseCode houseCode = HouseCode.NotSet;
            UnitCode unitCode = UnitCode.UnitNotSet;
            Command command = Command.NotSet;
            bool isCameraCommand = false;
            for (int i = 0; i < commands.Length; i++)
            {
                string cmd = commands[i].ToUpper();
                if (cmd.Length == 2)
                {
                    if (!Enum.TryParse(cmd[0].ToString(), out houseCode))
                    {
                        Console.WriteLine("Invalid house code.");
                        return;
                    }
                    if (cmd[1] == '+' || cmd[1] == '-')
                    {
                        switch (cmd[1])
                        {
                            case '+':
                                command = Command.Bright;
                                break;
                            case '-':
                                command = Command.Dim;
                                break;
                        }
                    }
                    else
                    {
                        isCameraCommand = true;
                        switch (cmd[1])
                        {
                            case 'U':
                                command = Command.CameraUp;
                                break;
                            case 'D':
                                command = Command.CameraDown;
                                break;
                            case 'L':
                                command = Command.CameraLeft;
                                break;
                            case 'R':
                                command = Command.CameraRight;
                                break;
                        }
                    }
                }
                else if (cmd.Length > 2)
                {
                    if (!Enum.TryParse(cmd[0].ToString(), out houseCode))
                    {
                        Console.WriteLine("Invalid house code.");
                        return;
                    }
                    if (!Enum.TryParse("Unit_"+cmd.Substring(1, cmd.Length - 2), out unitCode))
                    {
                        Console.WriteLine("Invalid unit number.");
                        return;
                    }
                    switch (cmd[cmd.Length-1])
                    {
                        case '+':
                            command = Command.On;
                            break;
                        case '-':
                            command = Command.Off;
                            break;
                        default:
                            Console.WriteLine("Invalid command.");
                            return;
                    }
                }
                if (isCameraCommand)
                {
                    Console.WriteLine("Sending X10 camera command '{0}' HouseCode '{1}'", command, houseCode);
                    cm19.SendCameraCommand(houseCode, command);
                    isCameraCommand = false;
                }
                else
                {
                    Console.WriteLine("Sending X10 command '{0}' HouseCode '{1}' Unit '{2}'", command, houseCode, unitCode);
                    cm19.SendCommand(houseCode, unitCode, command);
                }
                // pause 1 second between each command
                if (i < commands.Length - 1) Thread.Sleep(1000);
            }
        }
    }
}
