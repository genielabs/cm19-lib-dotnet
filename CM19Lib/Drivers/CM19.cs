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

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace CM19Lib.Drivers
{
    
    /// <summary>
    /// CM19 X10 RF transceiver driver.
    /// </summary>
    public class CM19 : XTenInterface, IDisposable
    {
        // Timeout
        private const int TransferTimeout = 1000;
        // Max size of each transfer
        private const int TransferSize = 8;
        // Max packet size
        private const int MaxPacketSize = 8;

        private readonly UsbDeviceFinder myUsbFinder = new UsbDeviceFinder(0x0BC7, 0x0002); // 0x0005 for CM21
        private UsbDevice myUsbDevice;

        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;

        /// <summary>
        /// Releases all resource used by the <see cref="CM19"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CM19"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CM19"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="CM19"/> so
        /// the garbage collector can reclaim the memory that the <see cref="CM19"/> was occupying.</remarks>
        public void Dispose()
        {
            if (myUsbDevice == null || !myUsbDevice.IsOpen) return;
            try
            {
                reader.Abort();
            }
            catch (Exception e)
            {
                X10RfManager.logger.Error(e);
            }
            try
            {
                writer.Abort();
            }
            catch (Exception e)
            {
                X10RfManager.logger.Error(e);
            }
        }

        /// <summary>
        /// Open the hardware interface.
        /// </summary>
        public bool Open()
        {
            bool success = true;
            //
            try
            {
                // Find and open the usb device.
                myUsbDevice = UsbDevice.OpenUsbDevice(myUsbFinder);
                // If the device is open and ready
                if (myUsbDevice == null)
                    throw new Exception("X10 CM19 device not connected.");
                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = myUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.
                    //
                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);
                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                    // Select alt interface #0
                    wholeUsbDevice.SetAltInterface(0);
                }

                Console.WriteLine(myUsbDevice.Info.ProductString);
                int eps = myUsbDevice.ActiveEndpoints.Count;
                for (int i = 0; i < eps; i++)
                {
                    Console.WriteLine("{0}:{1}", i, myUsbDevice.ActiveEndpoints[i].Type);
                }

                // open read endpoint 1.
                reader = myUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                // open write endpoint 2.
                writer = myUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Close the hardware interface.
        /// </summary>
        public void Close()
        {
            if (myUsbDevice == null) return;
            if (myUsbDevice.DriverMode == UsbDevice.DriverModeType.MonoLibUsb)
            {
                try
                {
                    myUsbDevice.Close();
                }
                catch (Exception e)
                {
                    X10RfManager.logger.Error(e);
                }
            }
            myUsbDevice = null;
            UsbDevice.Exit();
            Dispose();
        }
        
        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <returns>The data.</returns>
        public byte[] ReadData()
        {
            int transferredIn;
            UsbTransfer usbReadTransfer;
            //
            var readBuffer = new byte[MaxPacketSize];
            var ecRead = reader.SubmitAsyncTransfer(readBuffer, 0, TransferSize, TransferTimeout, out usbReadTransfer);
            if (ecRead != ErrorCode.None)
            {
                throw new Exception("Submit Async Read Failed.");
            }
            WaitHandle.WaitAll(new WaitHandle[] { usbReadTransfer.AsyncWaitHandle }, TransferTimeout, false);
            ecRead = usbReadTransfer.Wait(out transferredIn);

            if (!usbReadTransfer.IsCompleted)
            {
                ecRead = reader.SubmitAsyncTransfer(readBuffer, transferredIn, MaxPacketSize-transferredIn, TransferTimeout, out usbReadTransfer);
                if (ecRead != ErrorCode.None)
                {
                    throw new Exception("Submit Async Read Failed.");
                }
                WaitHandle.WaitAll(new WaitHandle[] { usbReadTransfer.AsyncWaitHandle }, TransferTimeout, false);
            }

            if (!usbReadTransfer.IsCompleted)
                usbReadTransfer.Cancel();

            try
            {
                ecRead = usbReadTransfer.Wait(out transferredIn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            usbReadTransfer.Dispose();

            byte[] readData = new byte[transferredIn];
            Array.Copy(readBuffer, readData, transferredIn);

            return readData;
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <returns><c>true</c>, if data was written, <c>false</c> otherwise.</returns>
        /// <param name="bytesToSend">Bytes to send.</param>
        public bool WriteData(byte[] bytesToSend)
        {
            if (myUsbDevice == null) return false;
            UsbTransfer usbWriteTransfer = null;
            var ecWrite = writer.SubmitAsyncTransfer(bytesToSend, 0, bytesToSend.Length, TransferTimeout, out usbWriteTransfer);
            if (ecWrite != ErrorCode.None)
            {
                throw new Exception("Submit Async Write Failed.");
            }

            WaitHandle.WaitAll(new WaitHandle[] { usbWriteTransfer.AsyncWaitHandle }, TransferTimeout, false);

            if (!usbWriteTransfer.IsCompleted)
                usbWriteTransfer.Cancel();
                
            int transferredOut;
            ecWrite = usbWriteTransfer.Wait(out transferredOut);
            usbWriteTransfer.Dispose();
            // TODO: should check if (transferredOut != bytesToSend.Length), and eventually resend?
            //Console.WriteLine(BitConverter.ToString(bytesToSend));
            return true;
        }        
    }
}


