using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace OPCClientCSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new OpcClient();

            client.Connect("localhost", "opcserversim.Instance.1");

            if (client.isConnected == true)
            {
                //client.Connect("localhost", "opcserversim.Instance.1");

                //client.Subscribe("Random.Int1");
                //client.Subscribe();
                //Console.WriteLine(client.Read("Bucket Brigade.ArrayOfReal8"));

                client.Browse();

                /*client.Write("StringValue", "adfasdgd");
                client.Write("BooleanValue", "1");
                client.Write("DateTimeValue", "11.11.2011 6:25:58");*/

                /*Console.WriteLine(client.Read("StringValue"));
                Console.WriteLine(client.Read("BooleanValue"));
                Console.WriteLine(client.Read("DateTimeValue"));*/

                Console.ReadKey();
                client.Disconnect();

            }

            Console.ReadKey();
        }
    }
}
