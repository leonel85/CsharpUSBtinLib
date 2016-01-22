using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpUSBTinLib
{
    public class CANMessage
    {
        #region private members
        int id;
        byte[] data;
        bool extended;
        bool rtr;
        #endregion private members

        #region public properties

        /// <summary>
        /// CAN message identifier
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        /// <summary>
        /// CAN message id is extended
        /// </summary>
        public bool isExtended
        {
            get { return extended; }
        }

        /// <summary>
        /// CAN message payload data
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        /// <summary>
        /// Number of bytes of data (0–8 bytes)
        /// </summary>
        public int DLC
        { 
            get { return data.Length; } 
        }

        /// <summary>
        /// Remote transmission request
        /// Must be dominant (0) for data frames and recessive (1) for remote request frames 
        /// </summary>
        public bool IsRTR
        {
            get { return rtr; }
        }

        #endregion public properties


        /// <summary>
        /// Create message with given id and data.
        /// Depending on Id, the extended flag is set.
        /// </summary>
        /// <param name="id">id Message identifier</param>
        /// <param name="data">data Payload data</param>
        public CANMessage(int id, byte[] data)
            : this(id, id > 0x7ff, false, data, data.Length)
        { }

        /// <summary>
        /// Create message with given message properties.
        /// </summary>
        /// <param name="id">Message identifier</param>
        /// <param name="extended">Marks messages with extended identifier</param>
        /// <param name="rtr">Marks RTR messages</param>
        /// <param name="data">Payload data</param>
        /// <param name="dlc">data length 0--8</param>
        public CANMessage(int id, bool extended, bool rtr, byte[] data, int dlc)
        {
            if (id > (0x1fffffff))
                id = 0x1fffffff;
            else
                this.id = id;

            this.data = new byte[dlc];
            for (int i = 0; i < dlc; i++)
                this.data[i] = data[i];
            this.extended = extended;
            this.rtr = rtr;
        }

        /// <summary>
        /// Create message with given message string.
        /// The message string is parsed. On errors, the corresponding value is
        /// set to zero. 
        /// 
        /// Example message strings:
        /// t1230        id: 123h        dlc: 0      data: --
        /// t00121122    id: 001h        dlc: 2      data: 11 22
        /// T12345678197 id: 12345678h   dlc: 1      data: 97
        /// r0037        id: 003h        dlc: 7      RTR
        /// </summary>
        /// <param name="msg">Message string</param>
        public CANMessage(String msg)
        {
            int index = 1;
            char type;
            if (msg.Length > 0)
                type = msg.First();
            else
                type = 't';

            extended = (type == 'T' || type == 'R');
            rtr = (type == 'r' || type == 'R');

            try
            {
                if (extended)
                {
                    this.id = int.Parse(msg.Substring(index, 8), System.Globalization.NumberStyles.HexNumber);
                    index += 8;
                }
                else
                {
                    this.id = int.Parse(msg.Substring(index, 3), System.Globalization.NumberStyles.HexNumber);
                    index += 3;
                }
            }
            catch(Exception e)
            {
                id = 0;
            }

            int length;

            try { length = int.Parse(msg.Substring(index, 1)); }
            catch (Exception e) { length = 0; }

            index += 1;

            this.data = new byte[length];        
            if (!this.rtr) {
                for (int i = 0; i < length; i++) 
                {
                    try 
                    {
                        this.data[i] = byte.Parse(msg.Substring(index, 2), System.Globalization.NumberStyles.HexNumber);
                    } 
                    catch (Exception e2) 
                    {
                        this.data[i] = 0;
                    } 
                    index += 2;
                }
            }
        }

        /// <summary>
        /// Message string Rapresentation
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            String s;
            if (extended)
            {
                if (this.rtr) s = "R";
                else s = "T";
                s = s + this.id.ToString("X8");
            }
            else
            {
                if (this.rtr) s = "r";
                else s = "t";
                s = s + this.id.ToString("X3");
            }
            s = s + String.Format("{0:X1}", this.data.Length);

            if (!this.rtr)
            {
                for (int i = 0; i < this.data.Length; i++)
                {
                    s = s + this.data[i].ToString("X2");
                }
            }
            return s;
        }
    }
}