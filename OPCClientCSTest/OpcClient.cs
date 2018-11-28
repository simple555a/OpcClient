using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Da;
using OpcCom;

namespace OPCClientCSTest
{
    class OpcClient
    {
        private Opc.Da.Server serverHandle;               // Переменая для работы с сервером
        public bool isConnected = false;                  // Статус подключения к серверу
        private Subscription subscription;
        private Factory factory = new Factory();

        /// <summary>
        /// Конструктор
        /// </summary>
        public OpcClient()
        {
            serverHandle = new Opc.Da.Server(factory, null);

            //subscriptionState.Active = false;
        }

        /// <summary>
        /// Подключение к серверу OPC. 
        /// </summary>
        public void Connect(string hostIp, string serverId)
        {
            // Создание URL
            //string url = "opcda://localhost/opcserversim.Instance.1";
            string url = "opcda://" + hostIp + "/" + serverId;
            var opcUrl = new Opc.URL(url);
            var connectData = new Opc.ConnectData(new System.Net.NetworkCredential());

            try
            {
                serverHandle.Connect(opcUrl, connectData);
                isConnected = true;
                Console.WriteLine("Connected to {0}", url);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to connect - status {0}", exception);
            }
        }

        /// <summary>
        /// Отключение от сервера OPC.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                serverHandle.Disconnect();
                isConnected = false;
                Console.WriteLine("Disconnect succeeded");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to disconnect - status {0}", exception);
            }
        }

        /// <summary>
        /// Чтение одного конкретного значения.
        /// В случае успешного чтения возвращает значение в формате string.
        /// Если чтение не удалось, возвращает пустую строку
        /// </summary>
        /// <param name="itemName"> название значения на OPC сервере </param>
        public string Read(string itemName)
        {
            var result = new ItemValueResult[1];

            var items = new Item[1];
            items[0] = new Opc.Da.Item
            {
                ItemName = itemName
            };
            try
            {
                result = serverHandle.Read(items);
                return result[0].Value.ToString();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Reading value failed - status: {0}", exception);
            }

            return "";
        }

        /// <summary>
        /// Запись одного конкретного значения
        /// </summary>
        public void Write(string itemName, string value)
        {
            var values = new ItemValue[1];
            values[0] = new ItemValue
            {
                ItemName = itemName,
                Value = value
            };
            serverHandle.Write(values);
        }

        /// <summary>
        /// Подписка на изменение значения на сервере
        /// </summary>
        public void Subscribe()
        {
            var subscriptionState = new SubscriptionState();
            subscriptionState.Name = "Group";
            subscriptionState.Active = true;
            subscription = (Opc.Da.Subscription)serverHandle.CreateSubscription(subscriptionState);

            Opc.Da.Item[] items = new Opc.Da.Item[3];

            items[0] = new Opc.Da.Item();
            items[0].ItemName = "StringValue";
            items[1] = new Opc.Da.Item();
            items[1].ItemName = "Random.Int1";
            items[2] = new Opc.Da.Item();
            items[2].ItemName = "Random.Time";

            items = subscription.AddItems(items);

            subscription.DataChanged += new Opc.Da.DataChangedEventHandler(DataChange);
        }

        /// <summary>
        /// Вспомогательный метод для подписок. Вызывается при срабатывании подписки
        /// </summary>
        /// <param name="group"></param>
        /// <param name="hReq"></param>
        /// <param name="items"></param>
        static void DataChange(object group, object hReq, Opc.Da.ItemValueResult[] items)
        {
            Console.WriteLine("-----------------");
            for (int i = 0; i < items.GetLength(0); i++)
            {
                Console.WriteLine("Item DataChange - ItemId: {0}", items[i].ItemName);
                Console.WriteLine(" Value: {0,-20}", items[i].Value);
                Console.WriteLine(" TimeStamp: {0:00}:{1:00}:{2:00}.{3:000}",
                items[i].Timestamp.Hour,
                items[i].Timestamp.Minute,
                items[i].Timestamp.Second,
                items[i].Timestamp.Millisecond);
            }
            Console.WriteLine("-----------------");
            Thread.Sleep(3000);
        }

        public void Browse(string itemName = "")
        {
            var itemIdentifier = new Opc.ItemIdentifier();
            itemIdentifier.ItemName = itemName;

            var browseFilters = new Opc.Da.BrowseFilters();
            browseFilters.BrowseFilter = browseFilter.all;

            var browsePosition = new Opc.Da.BrowsePosition(itemIdentifier, browseFilters);

            BrowseElement[] browseElements = serverHandle.Browse(itemIdentifier, browseFilters, out browsePosition);

            if (browseElements != null)
            {
                foreach (BrowseElement element in browseElements)
                {
                    if (element.IsItem)
                    {
                        Console.WriteLine("Item name : {0}", element.ItemName);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Group name : {0}", element.ItemName);
                        Browse(element.ItemName);
                    }
                }
            }
        }
    }
}
