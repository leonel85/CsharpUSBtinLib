using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsharpUSBTinLib
{
    public class GenericQueue<T>
    {
        public GenericQueue()
        {
            queda = new Queue<T>();
        }
        public T Read()
        {
            T ret;
            ret = default(T);

            Monitor.Enter(queda);
            while (queda.Count == 0) //attende finchè non ci sono elementi sulla coda
                Monitor.Wait(queda);
            ret = queda.Dequeue();
            Monitor.Exit(queda);
            return ret;
        }
        public void Write(T ms)
        {
            Monitor.Enter(queda);
            queda.Enqueue(ms);
            Monitor.Pulse(queda);
            Monitor.Exit(queda);
        }
        public Int32 Count()
        {
            Int32 n;
            Monitor.Enter(queda);
            n = queda.Count();
            Monitor.Exit(queda);
            return n;
        }
        private Queue<T> queda; //coda FIFO di qualunque tipo
    }
}
