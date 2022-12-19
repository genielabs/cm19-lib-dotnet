/*
  This file is part of CM19Lib (https://github.com/genielabs/cm19-lib-dotnet)
 
  Copyright (2012-2023) G-Labs (https://github.com/genielabs)

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

namespace CM19Lib.X10
{
    public enum UnitCode
    {
        UnitNotSet = 0xFF,
        Unit_1 = 0x000,
        Unit_2 = 0x010,
        Unit_3 = 0x008,
        Unit_4 = 0x018,
        Unit_5 = 0x040,
        Unit_6 = 0x050,
        Unit_7 = 0x048,
        Unit_8 = 0x058,
        Unit_9 = 0x400,
        Unit_10 = 0x410,
        Unit_11 = 0x408,
        Unit_12 = 0x418,
        Unit_13 = 0x440,
        Unit_14 = 0x450,
        Unit_15 = 0x448,
        Unit_16 = 0x458
    }

    namespace XTenLib
    {
        public static class X10UnitCodeExt
        {
            public static int Value(this UnitCode uc)
            {
                var parts = uc.ToString().Split('_');
                var unitCode = 0;
                int.TryParse(parts[1], out unitCode);
                return unitCode;
            }
        }

    }
    
    public static class X10UnitCodeExt
    {
        public static int Value(this UnitCode uc)
        {
            var parts = uc.ToString().Split('_');
            var unitCode = 0;
            int.TryParse(parts[1], out unitCode);
            return unitCode;
        }
    }

}

#pragma warning restore 1591
