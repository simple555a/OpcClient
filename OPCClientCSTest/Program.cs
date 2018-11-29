using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceProcess;

namespace OPCClientCSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new OpcClient();

            client.Connect();

            Thread writingValuesThread = new Thread(new ParameterizedThreadStart(LoopGetWriteValueFromController));
            writingValuesThread.Start(client);

            while (true)
            {
                client.SubscribeAll();
                Thread.Sleep(5000);
                client.UnsubscribeAll();
            }
        }

        public static void LoopGetWriteValueFromController(object opcClient)
        {
            OpcClient client = (OpcClient)opcClient;
            while (client.IsConnected())
            {
                client.GetWriteValueFromController();
                Thread.Sleep(10000);
            }
        }
    }
}
