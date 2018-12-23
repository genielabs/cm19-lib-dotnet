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
    public enum Function : int
    {
        /* Standard 5-byte commands: */
        On = 0x00,
        Off = 0x20,
        Dim = 0x98,
        Bright = 0x88,        
        AllLightsOn = 0x90,
        AllUnitsOff = 0x80,
        
        /* Pan'n'Tilt 4-byte commands: */
        CameraUp = 0x762,
        CameraRight = 0x661,
        CameraDown = 0x863,
        CameraLeft = 0x560,
        
        NotSet = 0xFF
    }
}

#pragma warning restore 1591
