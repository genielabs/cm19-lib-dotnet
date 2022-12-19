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

namespace CM19Lib.Events
{
    /// <summary>
    /// RF data received event arguments.
    /// </summary>
    public class RfDataReceivedEventArgs
    {
        /// <summary>
        /// The raw data.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfDataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="data">Data.</param>
        public RfDataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
