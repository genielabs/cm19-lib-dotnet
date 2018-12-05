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

using CM19Lib.Drivers;
using CM19Lib.Events;
using CM19Lib.X10;

using NLog;

namespace CM19Lib
{
    /// <summary>
    /// X10 Home Automation library for .NET / Mono. It supports CM11 (serial) and CM15 (USB) hardware.
    /// </summary>
    public class X10RfManager
    {
        #region Private Fields

        internal static Logger logger = LogManager.GetCurrentClassLogger();

        // X10 objects and configuration
        private const double X10DimmingStep = (100D / 224D);
        private XTenInterface x10Interface;

        // State variables
        private bool isInterfaceReady = false;

        // I/O operation lock / monitor
        private readonly object waitAckMonitor = new object();
        private readonly object commandLock = new object();

        // Variables used for preventing duplicated messages coming from RF
        private const uint MinRfRepeatDelayMs = 500;
        private DateTime lastReceivedTs = DateTime.Now;
        private DateTime lastRfReceivedTs = DateTime.Now;
        private string lastRfMessage = "";

        // Read/Write error state variable
        private bool gotReadWriteError = true;

        // X10 interface reader Task
        private Thread reader;

        // X10 interface connection watcher
        private Thread connectionWatcher;

        private readonly object accessLock = new object();
        private bool disconnectRequested = false;
        private int minRfRepeatDelayMs = 500;

        #endregion

        #region Public Events

        /// <summary>
        /// Connected state changed event.
        /// </summary>
        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusChangedEventArgs args);

        /// <summary>
        /// Occurs when connected state changed.
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        /// <summary>
        /// RF data received event.
        /// </summary>
        public delegate void RfDataReceivedEventHandler(object sender, RfDataReceivedEventArgs args);

        /// <summary>
        /// Occurs when RF data is received.
        /// </summary>
        public event RfDataReceivedEventHandler RfDataReceived;

        /// <summary>
        /// X10 command received event.
        /// </summary>
        public delegate void X10CommandReceivedEventHandler(object sender, RfCommandReceivedEventArgs args);

        /// <summary>
        /// Occurs when x10 command received.
        /// </summary>
        public event X10CommandReceivedEventHandler RfCommandReceived;

        /// <summary>
        /// X10 security data received event.
        /// </summary>
        public delegate void X10SecurityReceivedEventHandler(object sender, RfSecurityReceivedEventArgs args);

        /// <summary>
        /// Occurs when x10 security data is received.
        /// </summary>
        public event X10SecurityReceivedEventHandler RfSecurityReceived;

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="X10RfManager"/> class.
        /// </summary>
        public X10RfManager()
        {
            x10Interface = new Drivers.CM19();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="X10RfManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~X10RfManager()
        {
            Close();
        }

        #endregion

        #region Public Members

        #region X10 Configuration and Connection

        /// <summary>
        /// Connect to the X10 hardware.
        /// </summary>
        public bool Connect()
        {
            if (disconnectRequested)
                return false;
            lock (accessLock)
            {
                Disconnect();
                Open();
                connectionWatcher = new Thread(ConnectionWatcherTask);
                connectionWatcher.Start();
            }
            return IsConnected;
        }

        /// <summary>
        /// Connect from the X10 hardware.
        /// </summary>
        public void Disconnect()
        {
            if (disconnectRequested)
                return;
            disconnectRequested = true;
            Close();
            lock (accessLock)
            {
                if (connectionWatcher != null)
                {
                    if (!connectionWatcher.Join(5000))
                        connectionWatcher.Abort();
                    connectionWatcher = null;
                }
                disconnectRequested = false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the X10 hardware is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get
            {
                return (x10Interface != null && !disconnectRequested &&
                        (isInterfaceReady || (!gotReadWriteError /* && x10Interface.GetType().Equals(typeof(CM19))*/)));
            }
        }

        #endregion

        #region X10 Commands Implementation

        /// <summary>
        /// Dim the currently addressed modules.
        /// </summary>
        /// <param name="houseCode">House code.</param>
        public void Dim(HouseCode houseCode)
        {
            SendCommand(houseCode, 0x00, Command.Dim);
        }

        /// <summary>
        /// Brighten the curretly addressed module.
        /// </summary>
        /// <param name="houseCode">House code.</param>
        public void Bright(HouseCode houseCode)
        {
            SendCommand(houseCode, 0x00, Command.Bright);
        }

        /// <summary>
        /// Turn on the specified module (houseCode, unitCode).
        /// </summary>
        /// <param name="houseCode">House code.</param>
        /// <param name="unitCode">Unit code.</param>
        public void UnitOn(HouseCode houseCode, UnitCode unitCode)
        {
            SendCommand(houseCode, unitCode, Command.On);
        }

        /// <summary>
        /// Turn off the specified module (houseCode, unitCode).
        /// </summary>
        /// <param name="houseCode">House code.</param>
        /// <param name="unitCode">Unit code.</param>
        public void UnitOff(HouseCode houseCode, UnitCode unitCode)
        {
            SendCommand(houseCode, unitCode, Command.Off);
        }

        /// <summary>
        /// Turn on all the light modules with the given houseCode.
        /// </summary>
        /// <param name="houseCode">House code.</param>
        public void AllLightsOn(HouseCode houseCode)
        {
            SendCommand(houseCode, 0x00, Command.AllLightsOn);
        }

        /// <summary>
        /// Turn off all modules with the given houseCode.
        /// </summary>
        /// <param name="houseCode">House code.</param>
        public void AllUnitsOff(HouseCode houseCode)
        {
            SendCommand(houseCode, 0x00, Command.AllUnitsOff);
        }

        /// <summary>
        /// Turn off the specified module (houseCode, unitCode).
        /// </summary>
        /// <param name="houseCode">House code.</param>
        /// <param name="unitCode">Unit code.</param>
        /// <param name="command">X10 command.</param>
        /// <returns>`true' on success, `false` otherwise.</returns>
        public bool SendCommand(HouseCode houseCode, UnitCode unitCode, Command command)
        {
            int hu = ((int) unitCode >> 8) | (int) houseCode;
            int hc = ((int) unitCode & 0xFF) | (int) command;
            try
            {
                SendMessage(new[]
                {
                    (byte) RfCommandType.Standard,
                    (byte) hu,
                    (byte) ~hu,
                    (byte) hc,
                    (byte) ~hc
                });
            }
            catch (Exception e)
            {
                logger.Error(e);
                return false;
            }
            return true;
        }

        #endregion

        #endregion

        #region Private Members

        #region X10 Interface I/O operations

        /// <summary>
        /// Cm19Configuration class.
        /// </summary>
        public class Cm19Configuration
        {
            /// <summary>
            /// Init 0
            /// </summary>
            public string init0 = "00";
            /// <summary>
            /// Init 1
            /// </summary>
            public string init1 = "80-01-00-20-14-24-29-20";
            /// <summary>
            /// Init 2
            /// </summary>
            public string init2 = "";
        }

        private bool Open()
        {
            bool success;
            lock (accessLock)
            {
                Close();
                success = (x10Interface != null && x10Interface.Open());
                if (success)
                {
                    // Start the Reader task
                    gotReadWriteError = false;
                    // Start the Reader task
                    reader = new Thread(ReaderTask);
                    reader.Start();
                    //

                    /* CM19A initialization strings */
                    Cm19Configuration cfg = new Cm19Configuration();
                    try
                    {
                        cfg = Utility.ReadFromXmlFile<Cm19Configuration>("cm19_config.xml");
                    }
                    catch { }
                    //WriteToXmlFile("cm19_config.xml", cfg, false);
                    if (cfg.init0 != null && cfg.init0.Length > 0) SendMessage(Utility.StringToByteArray(cfg.init0));
                    if (cfg.init1 != null && cfg.init1.Length > 0) SendMessage(Utility.StringToByteArray(cfg.init1));
                    if (cfg.init2 != null && cfg.init2.Length > 0) SendMessage(Utility.StringToByteArray(cfg.init2));

                    // Fires the 'ConnectionStatusChanged' event
                    OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(true));
                }
            }
            return success;
        }

        private void Close()
        {
            lock (accessLock)
            {
                // Dispose the X10 interface
                try
                {
                    x10Interface.Close();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
                gotReadWriteError = true;
                // Stop the Reader task
                if (reader != null)
                {
                    if (!reader.Join(5000))
                        reader.Abort();
                    reader = null;
                }
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(false));
            }
        }

        /// <summary>
        /// Sends a raw RF message.
        /// </summary>
        /// <param name="message">The raw message.</param>
        public void SendMessage(byte[] message)
        {
            lock (waitAckMonitor)
                try
                {
                    logger.Debug(BitConverter.ToString(message));
                    if (!x10Interface.WriteData(message))
                    {
                        logger.Warn("Interface I/O error");
                    }
                    else
                    {
                        Monitor.Pulse(waitAckMonitor);
                        Monitor.Wait(waitAckMonitor, 1000);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    gotReadWriteError = true;
                }
        }

        private void ReaderTask()
        {
            while (x10Interface != null && !disconnectRequested)
            {
                try
                {
                    byte[] readData = x10Interface.ReadData();
                    if (readData.Length == 1 && (readData[0] == 0xFF || readData[0] == 0x00))
                    {
                        if (readData[0] == 0x00)
                        {
                            logger.Warn("NACK");
                        }
                        lock (waitAckMonitor)
                            Monitor.Pulse(waitAckMonitor);
                    }
                    else if (readData.Length > 1)
                    {
                        logger.Debug(BitConverter.ToString(readData));
                        {
                            lastReceivedTs = DateTime.Now;
                            byte[] message = readData;
                            bool isSecurityCode = (message[0] == (byte)RfCommandType.Security) && 
                                                  (message.Length == 7 && ((message[2] ^ message[1]) == 0x0F) &&
                                                   ((message[4] ^ message[3]) == 0xFF));
                            bool isCodeValid = isSecurityCode || (message[0] == (byte)RfCommandType.Standard) && 
                                               (message.Length == 5 &&
                                                ((message[2] & ~message[1]) == message[2] &&
                                                 (message[4] & ~message[3]) == message[4]));                            

                            // Repeated messages check
                            if (isCodeValid)
                            {
                                if (lastRfMessage == BitConverter.ToString(message) &&
                                    (DateTime.Now - lastRfReceivedTs).TotalMilliseconds < minRfRepeatDelayMs)
                                {
                                    logger.Warn("Ignoring repeated message within {0}ms", minRfRepeatDelayMs);
                                    continue;
                                }
                                lastRfMessage = BitConverter.ToString(message);
                                lastRfReceivedTs = DateTime.Now;
                            }

                            OnRfDataReceived(new RfDataReceivedEventArgs(message));

                            if (isSecurityCode)
                            {
                                var securityEvent = RfSecurityEvent.NotSet;
                                Enum.TryParse<RfSecurityEvent>(message[3].ToString(), out securityEvent);
                                uint securityAddress = message[1];
                                if (securityEvent != RfSecurityEvent.NotSet)
                                {
                                    logger.Debug("Security Event {0} Address {1}", securityEvent, securityAddress);
                                    OnRfSecurityReceived(new RfSecurityReceivedEventArgs(securityEvent, securityAddress));
                                }
                                else
                                {
                                    logger.Warn("Could not parse security event");
                                }
                            }
                            // TODO: implement code for type RfCommandType.Camera
                            else if (isCodeValid)
                            {
                                // Decode received 32 bit message
                                // house code + 4th bit of unit code
                                // unit code (3 bits) + function code

                                // Parse function code
                                var hf = RfFunction.NotSet;
                                Enum.TryParse<RfFunction>(message[3].ToString(), out hf);
                                // House code (4bit) + unit code (4bit)
                                byte hu = message[1];
                                // Parse house code
                                var houseCode = (HouseCode) (message[1] & 0xF0);
                                //Enum.TryParse<HouseCode>((Utility.ReverseByte((byte) (hu >> 4)) >> 4).ToString(), out houseCode);
                                switch (hf)
                                {
                                    case RfFunction.Dim:
                                    case RfFunction.Bright:
                                        logger.Debug("Command {0} HouseCode {1}", hf, houseCode);
                                        OnRfCommandReceived(new RfCommandReceivedEventArgs(hf, houseCode,
                                            UnitCode.UnitNotSet));
                                        break;
                                    case RfFunction.AllLightsOn:
                                    case RfFunction.AllLightsOff:
                                        if (houseCode != HouseCode.NotSet)
                                        {
                                            logger.Debug("Command {0} HouseCode {1}", hf, houseCode);
                                            OnRfCommandReceived(new RfCommandReceivedEventArgs(hf, houseCode,
                                                UnitCode.UnitNotSet));
                                        }
                                        break;
                                    case RfFunction.NotSet:
                                        logger.Warn("Unable to decode function value");
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
                                        var fn = RfFunction.NotSet;
                                        Enum.TryParse<RfFunction>(unitFunction[2].ToString(), out fn);
                                        switch (fn)
                                        {
                                            case RfFunction.On:
                                            case RfFunction.Off:
                                                var unitCode = UnitCode.UnitNotSet;
                                                Enum.TryParse<UnitCode>("Unit_"+uc.ToString(), out unitCode);
                                                if (unitCode != UnitCode.UnitNotSet)
                                                {
                                                    logger.Debug("Command {0} HouseCode {1} UnitCode {2}", fn,
                                                        houseCode, unitCode.ToString().Replace("Unit_", ""));
                                                    OnRfCommandReceived(
                                                        new RfCommandReceivedEventArgs(fn, houseCode, unitCode));
                                                }
                                                else
                                                {
                                                    logger.Warn("Could not parse unit code");
                                                }
                                                break;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                logger.Warn("Bad message received");
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    if (!e.GetType().Equals(typeof(TimeoutException)) && !e.GetType().Equals(typeof(OverflowException)))
                    {
                        gotReadWriteError = true;
                        logger.Error(e);
                    }
                }
            }
        }

        private void ConnectionWatcherTask()
        {
            // This task takes care of automatically reconnecting the interface
            // when the connection is drop or if an I/O error occurs
            while (!disconnectRequested)
            {
                if (gotReadWriteError)
                {
                    try
                    {
                        Close();
                        // wait 3 secs before reconnecting
                        Thread.Sleep(3000);
                        if (!disconnectRequested)
                        {
                            try
                            {
                                Open();
                            }
                            catch (Exception e)
                            {
                                logger.Error(e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }
                if (!disconnectRequested)
                    Thread.Sleep(1000);
            }
        }

        #endregion

        #region Events Raising

        /// <summary>
        /// Raises the connected state changed event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs args)
        {
            // ensure the status is really changing
            if (isInterfaceReady != args.Connected)
            {
                isInterfaceReady = args.Connected;
                // raise the event
                ConnectionStatusChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Raises the rf data received event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnRfDataReceived(RfDataReceivedEventArgs args)
        {
            if (RfDataReceived != null)
                RfDataReceived(this, args);
        }

        /// <summary>
        /// Raises the RF command received event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnRfCommandReceived(RfCommandReceivedEventArgs args)
        {
            if (RfCommandReceived != null)
                RfCommandReceived(this, args);
        }

        /// <summary>
        /// Raises the RF security received event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnRfSecurityReceived(RfSecurityReceivedEventArgs args)
        {
            if (RfSecurityReceived != null)
                RfSecurityReceived(this, args);
        }

        #endregion

        #endregion
    }
}
