using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpUSBTinLib {
  public class Shared {
    public enum OpenMode {
      ACTIVE = 0,
      LISTENONLY = 1,
      LOOPBACK = 2,
      LOOPBACK_LISTENONLY = 3
    }

    public enum CANBusSpeed {
      Speed_10000 = 10000,
      Speed_20000 = 20000,
      Speed_50000 = 50000,
      Speed_100000 = 100000,
      Speed_125000 = 125000,
      Speed_250000 = 250000,
      Speed_500000 = 500000,
      Speed_800000 = 800000,
      Speed_1000000 = 1000000,
      Speed_Custom = 0
    }
  }
}
