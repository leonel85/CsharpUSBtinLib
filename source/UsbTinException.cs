using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpUSBTinLib {
  public class UsbTinException : Exception {
    public UsbTinException()
       : base() { }

    public UsbTinException(Exception ex)
       : base(ex.Message) { }

    public UsbTinException(string message)
         : base(message) { }

    public UsbTinException(string format, params object[] args)
        : base(string.Format(format, args)) { }

    public UsbTinException(string message, Exception innerException)
        : base(message, innerException) { }

    public UsbTinException(string format, Exception innerException, params object[] args)
        : base(string.Format(format, args), innerException) { }
  }
}
