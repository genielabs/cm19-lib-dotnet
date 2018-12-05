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
using System.IO;
using System.Xml.Serialization;

using CM19Lib.X10;

namespace CM19Lib
{
    
    public static class Utility
    {
        private const int X10_MAX_DIM = 22;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static byte GetDimValue(int percentage)
        {
            if (percentage > 100)
                percentage = 100;
            else if (percentage < 0)
                percentage = 0;
            percentage = (int)Math.Floor(((double)percentage / 100D) * X10_MAX_DIM);
            byte dimvalue = (byte)(percentage << 3);
            return dimvalue;
        }

        /// <summary>
        /// returns a value between 0.0 and 1.0 representing the percentage of dim
        /// </summary>
        /// <param name="dimvalue"></param>
        /// <returns></returns>
        public static double GetPercentageValue(byte dimvalue)
        {
            return Math.Round(((double)((dimvalue) >> 3) / (double)X10_MAX_DIM), 2);
        }

        public static string HouseUnitCodeFromEnum(HouseCode housecode, UnitCode unitcodes)
        {
            string unit = unitcodes.ToString();
            unit = unit.Substring(unit.LastIndexOf("_") + 1);
            //
            return housecode.ToString() + unit;
        }

        public static HouseCode HouseCodeFromString(string s)
        {
            var houseCode = HouseCode.A;
            s = s.Substring(0, 1).ToUpper();
            switch (s)
            {
            case "A":
                houseCode = HouseCode.A;
                break;
            case "B":
                houseCode = HouseCode.B;
                break;
            case "C":
                houseCode = HouseCode.C;
                break;
            case "D":
                houseCode = HouseCode.D;
                break;
            case "E":
                houseCode = HouseCode.E;
                break;
            case "F":
                houseCode = HouseCode.F;
                break;
            case "G":
                houseCode = HouseCode.G;
                break;
            case "H":
                houseCode = HouseCode.H;
                break;
            case "I":
                houseCode = HouseCode.I;
                break;
            case "J":
                houseCode = HouseCode.J;
                break;
            case "K":
                houseCode = HouseCode.K;
                break;
            case "L":
                houseCode = HouseCode.L;
                break;
            case "M":
                houseCode = HouseCode.M;
                break;
            case "N":
                houseCode = HouseCode.N;
                break;
            case "O":
                houseCode = HouseCode.O;
                break;
            case "P":
                houseCode = HouseCode.P;
                break;
            }
            return houseCode;
        }

        public static UnitCode UnitCodeFromString(string s)
        {
            var unitCode = UnitCode.Unit_1;
            s = s.Substring(1);
            switch (s)
            {
            case "1":
                unitCode = UnitCode.Unit_1;
                break;
            case "2":
                unitCode = UnitCode.Unit_2;
                break;
            case "3":
                unitCode = UnitCode.Unit_3;
                break;
            case "4":
                unitCode = UnitCode.Unit_4;
                break;
            case "5":
                unitCode = UnitCode.Unit_5;
                break;
            case "6":
                unitCode = UnitCode.Unit_6;
                break;
            case "7":
                unitCode = UnitCode.Unit_7;
                break;
            case "8":
                unitCode = UnitCode.Unit_8;
                break;
            case "9":
                unitCode = UnitCode.Unit_9;
                break;
            case "10":
                unitCode = UnitCode.Unit_10;
                break;
            case "11":
                unitCode = UnitCode.Unit_11;
                break;
            case "12":
                unitCode = UnitCode.Unit_12;
                break;
            case "13":
                unitCode = UnitCode.Unit_13;
                break;
            case "14":
                unitCode = UnitCode.Unit_14;
                break;
            case "15":
                unitCode = UnitCode.Unit_15;
                break;
            case "16":
                unitCode = UnitCode.Unit_16;
                break;
            }
            return unitCode;
        }

        public static byte ReverseByte(byte originalByte)
        {
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = result << 1;
                result += originalByte & 1;
                originalByte = (byte)(originalByte >> 1);
            }
            return (byte)result;
        }
        
        /// <summary>
        /// Convert string to Byte array.
        /// </summary>
        /// <param name="hex">Hex bytes</param>
        /// <returns></returns>
        public static byte[] StringToByteArray(String hex)
        {
            hex = hex.Replace("-", "");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

    }
}

#pragma warning restore 1591
