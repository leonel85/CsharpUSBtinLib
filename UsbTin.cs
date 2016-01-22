using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsharpUSBTinLib
{
    public class UsbTin
    {
        GenericQueue<string> Q_Receive;
        GenericQueue<CANMessage> Q_Send;
        Thread Th_Send, Th_Receive;
        AutoResetEvent autoEvent;
        object SyncPort = new object();

        SerialPort serialPort;
        String firmwareVersion;
        String hardwareVersion;


        public UsbTin()
        {
            autoEvent = new AutoResetEvent(false);
            Q_Receive = new GenericQueue<string>();
            Th_Receive = new Thread(ThD_Receive);
            Th_Receive.IsBackground = true;


            Q_Send = new GenericQueue<CANMessage>();
            Th_Send = new Thread(ThD_Send);
            Th_Send.IsBackground = true;

            Th_Receive.Start();
            Th_Send.Start();
        }

        #region private functions

        /// <summary>
        /// Send Thread
        /// </summary>
        /// <param name="o"></param>
        private void ThD_Send(object o)
        {
            CANMessage msg;
            bool result;
            while (true)
            {
                msg = Q_Send.Read();

                lock (SyncPort)
                    serialPort.Write(msg.ToString() + "\r");
                
                result = autoEvent.WaitOne(100);
                if (result)
                {
                    RaiseMessage("Writed \t \t msg = " + msg.ToString());
                }
                else
                {
                    RaiseMessage("Write Error \t msg = " + msg.ToString());
                }
            }
        }

        /// <summary>
        /// Input thread
        /// </summary>
        /// <param name="o"></param>
        private void ThD_Receive(object o)
        {
            string msg;
            while (true)
            {
                msg = Q_Receive.Read();
                char cmd = msg[0];

                RaiseMessage("NEW MSG \t " + msg);

                if (msg.EndsWith("\r") && (cmd == 't' || cmd == 'T' || cmd == 'r' || cmd == 'R'))
                    RaiseCANMessage(new CANMessage(msg));
                else
                {
                    if (msg == "z\r" || msg == "Z\r" || msg == "t\r" || msg == "T\r")
                        autoEvent.Set();
                    else
                        RaiseMessage("Unknown msg: \t" + msg);
                }
            }
        }

        #endregion



        #region events
        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler MessageEvent;

        public delegate void CANMessageEventHandler(CANMessage message);
        public event CANMessageEventHandler CANMessageEvent;

        private void RaiseMessage(string msg)
        {
            if (MessageEvent != null) MessageEvent(msg);
        }

        private void RaiseCANMessage(CANMessage msg)
        {
            if (CANMessageEvent != null) CANMessageEvent(msg);
        }

        #endregion  events

        /// <summary>
        /// Connect to USBtin on given port.
        /// Opens the serial port, clears pending characters and send close command
        /// to make sure that we are in configuration mode.
        /// </summary>
        /// <param name="portName">Name of virtual serial port</param>
        /// <returns></returns>
        public bool Connect(String portName)
        {
            bool conected = false;
            try
            {
                // create serial port object
                serialPort = new SerialPort(portName);

                // initialize
                serialPort.BaudRate = 115200;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;
                serialPort.ReadTimeout = 1000;

                System.Threading.Thread.Sleep(500);

                serialPort.Open();


                // clear port and make sure we are in configuration mode (close cmd)
                serialPort.Write("\rC\r");

                System.Threading.Thread.Sleep(100);

                //(SerialPort.PURGE_RXCLEAR | SerialPort.PURGE_TXCLEAR);
                //•PURGE_TXABORT  immediately stops all write operations even if they are not finished;
                //•PURGE_RXABORT  immediately stops all read operations even if they are not finished;
                //•PURGE_TXCLEAR  clears the out -queue in the driver;
                //•PURGE_RXCLEAR  clears the in -queue in the driver.

                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                serialPort.Write("C\r");
                System.Threading.Thread.Sleep(100);



                int b;
                do
                {
                    b = serialPort.ReadByte();
                } while ((b != '\r') && (b != 7));

                // get version strings
                firmwareVersion = Transmit("v").Substring(1);
                hardwareVersion = Transmit("V").Substring(1);

                // reset overflow error flags
                Transmit("W2D00");

                conected = true;

            }
            catch (TimeoutException te)
            {
                RaiseMessage("Timeout! USBtin doesn't answer. Right port?");
            }
            catch (Exception e)
            {
                throw new UsbTinException("Connect \t" + e.Message);
            }
            /*TODO catch   ( SerialPor SerialPortException e) {
                throw new USBtinException(e.getPortName() + " - " + e.getExceptionType());
            } catch (SerialPortTimeoutException e) {
                throw new USBtinException("Timeout! USBtin doesn't answer. Right port?");
            } catch (InterruptedException e) {
                throw new USBtinException(e);
            } */
            return conected;
        }


        public List<string> GetDevives()
        {
            return SerialPort.GetPortNames().ToList();
        }


        /**
     * Disconnect.
     * Close serial port connection
     * 
     * @throws USBtinException Error while closing connection
     */
        public void Disconnect()
        {

            try
            {
                serialPort.Close();
            }
            catch (Exception e)
            {
                throw new UsbTinException(e.Message);
            }
        }


        /**
     * Transmit given command to USBtin
     *
     * @param cmd Command
     * @return Response from USBtin
     * @throws SerialPortException Error while talking to USBtin
     * @throws SerialPortTimeoutException Timeout of serial port
     */
        public String Transmit(String cmd)
        {
            String cmdline = cmd + "\r";
            serialPort.Write(cmdline);
            return ReadResponse();
        }


        private String ReadResponse() 
        {
            System.Threading.Thread.Sleep(100);
            string s = serialPort.ReadExisting();

            return s;
        //           StringBuilder response = new StringBuilder();
        //while (true) {
        //    byte[] buffer = serialPort.re.rea.readBytes(1, 1000);
        //    if (buffer[0] == '\r') {
        //        return response.toString();
        //    } else if (buffer[0] == 7) {
        //        throw new SerialPortException(serialPort.getPortName(), "transmit", "BELL signal");
        //    } else {
        //        response.append((char) buffer[0]);
        //    }
        }
   
        /**
             * Open CAN channel.
             * Set given baudrate and open the CAN channel in given mode.
             * 
             * @param baudrate Baudrate in bits/second
             * @param mode CAN bus accessing mode
             * @throws USBtinException Error while opening CAN channel
             */
        public void OpenCANChannel(int baudrate, Shared.OpenMode mode)
        {
            try
            {
                // set baudrate
                char baudCh = ' ';
                switch (baudrate)
                {
                    case 10000: baudCh = '0'; break;
                    case 20000: baudCh = '1'; break;
                    case 50000: baudCh = '2'; break;
                    case 100000: baudCh = '3'; break;
                    case 125000: baudCh = '4'; break;
                    case 250000: baudCh = '5'; break;
                    case 500000: baudCh = '6'; break;
                    case 800000: baudCh = '7'; break;
                    case 1000000: baudCh = '8'; break;
                }

                if (baudCh != ' ')
                {
                    // use preset baudrate               
                    Transmit("S" + baudCh);
                }
                else
                {
                    // calculate baudrate register settings

                    /*TODO  int FOSC = 24000000;
                      int xdesired = FOSC / baudrate;
                      int xopt = 0;
                      int diffopt = 0;
                      int brpopt = 0;

                      // walk through possible can bit length (in TQ)
                      for (int x = 11; x <= 23; x++) {

                          // get next even value for baudrate factor
                          int xbrp = (xdesired * 10) / x;
                          int m = xbrp % 20;
                          if (m >= 10) xbrp += 20;
                          xbrp -= m;
                          xbrp /= 10;

                          // check bounds
                          if (xbrp < 2) xbrp = 2;
                          if (xbrp > 130) xbrp = 130;

                          // calculate diff
                          int xist = x * xbrp;
                          int diff = xdesired - xist;
                          if (diff < 0) diff = -diff;

                          // use this clock option if it is better than previous
                          if ((xopt == 0) || (diff <= diffopt)) { xopt = x; diffopt = diff; brpopt = xbrp / 2 - 1;};
                      }

                      // mapping for CNF register values
                      int[] cnfvalues = new int[] {0x9203, 0x9303, 0x9B03, 0x9B04, 0x9C04, 0xA404, 0xA405, 0xAC05, 0xAC06, 0xAD06, 0xB506, 0xB507, 0xBD07};

                      Transmit("s" + String.format("%02x", brpopt | 0xC0) + String.format("%04x", cnfvalues[xopt - 11]));
                
                      System.out.println("No preset for given baudrate " + baudrate + ". Set baudrate to " + (FOSC / ((brpopt + 1) * 2) / xopt));
                      */

                    throw new UsbTinException("TODO");
                }

                // open can channel
                char modeCh = 'O';
                switch (mode)
                {
                    case Shared.OpenMode.LISTENONLY: modeCh = 'L'; break;
                    case Shared.OpenMode.LOOPBACK: modeCh = 'l'; break;
                    case Shared.OpenMode.ACTIVE: modeCh = 'O'; break;

                    //TODO default:
                    //    System.err.println("Mode " + mode + " not supported. Opening listen only.");
                }

                Transmit(modeCh + "");

                // register serial port event listener
                //serialPort.setEventsMask(SerialPort.MASK_RXCHAR);
                //serialPort.addEventListener(this);
                serialPort.DataReceived += serialPort_DataReceived;

            }
            catch (Exception e)
            {
                throw new UsbTinException("TODO");
            }
            /*catch (SerialPortException e) {
                throw new USBtinException(e);
            } catch (SerialPortTimeoutException e) {
                throw new USBtinException("Timeout! USBtin doesn't answer. Right port?");            
            }*/
        }

        /**
     * Close CAN channel.
     * 
     * @throws USBtinException Error while closing CAN channel
     */
        public void CloseCANChannel()
        {
            try
            {
                serialPort.DataReceived -= serialPort_DataReceived;
                serialPort.Write("C\r");
            }
            catch (Exception e)
            {
                throw new UsbTinException(e.Message);
            }

            firmwareVersion = null;
            hardwareVersion = null;
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (SyncPort)
                Q_Receive.Write(serialPort.ReadExisting());
        }

        public void Send(CANMessage canmsg)
        {
            Q_Send.Write(canmsg);
        }

    }
}
