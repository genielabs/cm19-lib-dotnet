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

namespace CM19Lib.X10
{
    public enum HouseCode
    {
        NotSet = 0xFF,
        A = 0x60,
        B = 0x70,
        C = 0x40,
        D = 0x50,
        E = 0x80,
        F = 0x90,
        G = 0xA0,
        H = 0xB0,
        I = 0xE0,
        J = 0xF0,
        K = 0xC0,
        L = 0xD0,
        M = 0x00,
        N = 0x10,
        O = 0x20,
        P = 0x30
    }
}

#pragma warning restore 1591
