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

            if (client.isConnected)
            {
                client.Browse();

                for (int i=0; i<10; i++)
                {
                    Console.WriteLine("i = {0}", i);
                    client.SubscribeAll();
                    client.UnsubscribeAll();
                    Thread.Sleep(5000);
                }

                //client.SubscribeAll();

                client.ReadAndSave("IntegerValue");
                Console.ReadKey();
                client.ReadAndSave("IntegerValue");

                Console.WriteLine("Press Enter to disconnect");
                Console.ReadKey();
                client.Disconnect();
            }

            Console.ReadKey();
        }
    }
}
