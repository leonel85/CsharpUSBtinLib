# CsharpUSBtinLib
A C# library for talking to the Usbtin USB CAN adapter


USBtin is a simple USB to CAN interface. It can monitor CAN busses and transmit CAN messages. 
USBtin implements the USB CDC class and creates a virtual serial port on the host computer.

This C# library parses the data received on the serial port into CAN message
format and can also generate and send CAN messages via the same port.  

I take no credit for originality - this library is a fork from
https://github.com/NeloHuerta/CsharpUSBtinLib
which in turn is a fork from the original
https://github.com/leonel85/CsharpUSBtinLib

## 2021-01-22
Forked from NeloHuerta. Move to VS 2019. Add README file. Minor spelling corrections and code tidy-up.
Expose Connected property as a public read-only property. 
