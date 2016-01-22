using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpUSBTinLib
{
    public class Shared
    {
        public enum OpenMode
        {
            ACTIVE = 0,
            LISTENONLY = 1,
            LOOPBACK = 2
        }
    }
}
