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

#pragma warning disable 1591

using System;

namespace CM19Lib.X10
{

    public enum RfMessageType
    {
        NotSet = 0xFF,
        Camera = 0x14,
        Standard = 0x20,
        StandardAlt = 0x24,
        Security = 0x29
    }

    public class RfMessage
    {
        /// <summary>
        /// X10 message type (Standard 0x20, Security 0x29, Camera 0x24, ...)
        /// </summary>
        public RfMessageType MessageType = RfMessageType.NotSet;

        public RfSecurityEvent SecurityEvent = RfSecurityEvent.NotSet;
        public uint SecurityAddress;
        public HouseCode HouseCode = HouseCode.NotSet;
        public UnitCode Unit = UnitCode.UnitNotSet;
        public Function Function = Function.NotSet;

        /// <summary>
        /// Parse a raw RF X10 message
        /// </summary>
        /// <param name="message">Hex string representation of the 4 bytes X10 RF message.</param>
        /// <returns>The decoded message as RfMessage class instance.</returns>
        public static RfMessage Decode(string message)
        {
            return Decode(Utility.StringToByteArray(message));
        }

        /// <summary>
        /// Parse a raw RF X10 message
        /// </summary>
        /// <param name="message">The binary message (4 bytes X10 RF message).</param>
        /// <returns>The decoded message as RfMessage class instance.</returns>
        public static RfMessage Decode(byte[] message)
        {
            bool isSecurityCode = (message[0] == (byte) RfMessageType.Security) &&
                                  (message.Length == 7 && ((message[2] ^ message[1]) == 0x0F) &&
                                   ((message[4] ^ message[3]) == 0xFF));
            bool isCameraCode = (message[0] == (byte) RfMessageType.Security);
            bool isStandardCode = (message[0] == (byte) RfMessageType.Standard) &&
                                  (message.Length == 5 &&
                                   ((message[2] & ~message[1]) == message[2] &&
                                    (message[4] & ~message[3]) == message[4]));
            bool isCodeValid = isStandardCode || isCameraCode || isSecurityCode;

            var msg = new RfMessage();
            if (isCodeValid)
            {
                msg.MessageType = isSecurityCode
                    ? RfMessageType.Security
                    : (isCameraCode ? RfMessageType.Camera : RfMessageType.Standard);
            }
            else return msg;

            if (isSecurityCode)
            {
                var securityEvent = RfSecurityEvent.NotSet;
                Enum.TryParse<RfSecurityEvent>(message[3].ToString(), out securityEvent);
                uint securityAddress = message[1];
                msg.SecurityEvent = securityEvent;
                msg.SecurityAddress = securityAddress;
            }
            else if (isCameraCode)
            {
                var houseCode = (HouseCode) (message[1] & 0xF0);
                var command = (Function) (((message[1] & 0xF) << 8) | message[2]);
                msg.HouseCode = houseCode;
                msg.Function = command;
            }
            else if (isStandardCode)
            {
                // Decode received 32 bit message
                // house code + 4th bit of unit code
                // unit code (3 bits) + function code

                // Parse function code
                var hf = Function.NotSet;
                Enum.TryParse<Function>(message[3].ToString(), out hf);
                // House code (4bit) + unit code (4bit)
                byte hu = message[1];
                // Parse house code
                var houseCode = (HouseCode) (message[1] & 0xF0);
                //Enum.TryParse<HouseCode>((Utility.ReverseByte((byte) (hu >> 4)) >> 4).ToString(), out houseCode);
                msg.HouseCode = houseCode;
                msg.Function = hf;
                switch (hf)
                {
                    case Function.Dim:
                    case Function.Bright:
                        break;
                    case Function.AllLightsOn:
                    case Function.AllUnitsOff:
                        break;
                    case Function.NotSet:
                        break;
                    default:
                        // Parse unit code
                        string houseUnit = Convert.ToString(hu, 2).PadLeft(8, '0');
                        string unitFunction = Convert.ToString(message[3], 2).PadLeft(8, '0');
                        string uc = (Convert.ToInt16(
                                         houseUnit.Substring(5, 1) + unitFunction.Substring(1, 1) +
                                         unitFunction.Substring(4, 1) + unitFunction.Substring(3, 1),
                                         2) + 1).ToString();
                        // Parse module function
                        var fn = (unitFunction[2] == '1' ? Function.Off : Function.On);
                        msg.Function = fn;
                        switch (fn)
                        {
                            case Function.On:
                            case Function.Off:
                                var unitCode = UnitCode.UnitNotSet;
                                Enum.TryParse<UnitCode>("Unit_" + uc.ToString(), out unitCode);
                                msg.Unit = unitCode;
                                break;
                        }
                        break;
                }
            }

            return msg;
        }

    }

}

#pragma warning restore 1591
