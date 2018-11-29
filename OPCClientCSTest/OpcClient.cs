using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Da;
using OpcCom;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OPCClientCSTest
{
    class OpcClient
    {
        private Opc.Da.Server serverHandle;               // Переменая для работы с сервером
        private Subscription subscription;                // Объект, содержащий информацию о подписках
        private Factory factory = new Factory();
        private OpcClientConfig config = new OpcClientConfig(); // Объет, содержащий конфигурационные данные
        private List<Opc.Da.Item> tagList = new List<Opc.Da.Item>();      // Список названий всех тегов

        /// <summary>
        /// Конструктор
        /// </summary>
        public OpcClient()
        {
            serverHandle = new Opc.Da.Server(factory, null);
            config.GetConfig();
            //subscriptionState.Active = false;
        }

        /// <summary>
        /// Подключение к серверу OPC. Данные для подключения берутся из конфигурационного файла. 
        /// </summary>
        public void Connect()
        {
            // Создание URL
            string url = "opcda://localhost/" + config.opcServerId;
            //string url = "opcda://" + config.amicumIp + "/" + config.opcServerId;
            var opcUrl = new Opc.URL(url);
            var connectData = new Opc.ConnectData(new System.Net.NetworkCredential());

            try
            {
                serverHandle.Connect(opcUrl, connectData);
                InitTagList();
                Console.WriteLine("Connected to {0}", url);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to connect - status {0}", exception);
            }
        }

        /// <summary>
        /// Подключение к серверу OPC. 
        /// </summary>
        /// <overloads> </overloads>
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
                Console.WriteLine("Disconnect succeeded");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to disconnect - status {0}", exception);
            }
        }

        /// <summary>
        /// Проверка на соединение с сервером ОРС
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return serverHandle.IsConnected;
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
        /// Чтение одного конкретного значения.
        /// В случае успешного чтения возвращает значение в формате string.
        /// Если чтение не удалось, возвращает пустую строку
        /// </summary>
        /// <param name="itemName"> название значения на OPC сервере </param>
        public string ReadAndSave(string itemName)
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

                SendItemValueRequest(result[0]);

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
        public void SubscribeAll()
        {
            var subscriptionState = new SubscriptionState
            {
                Name = "All",
                Active = true
            };

            subscription = (Opc.Da.Subscription)serverHandle.CreateSubscription(subscriptionState);

            Opc.Da.Item[] items = tagList.ToArray();

            items = subscription.AddItems(items);

            subscription.DataChanged += new Opc.Da.DataChangedEventHandler(DataChange);
        }

        /// <summary>
        /// Подписка на изменение конкретного значения на сервере
        /// </summary>
        public void Subscribe(string itemName)
        {
            var subscriptionState = new SubscriptionState
            {
                Name = itemName,
                Active = true
            };

            try
            {
                subscription = (Opc.Da.Subscription)serverHandle.CreateSubscription(subscriptionState);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error {0}", exception.Message);
                return;
            }
            
            
            Opc.Da.Item[] items = new Opc.Da.Item[1];

            items[0] = new Opc.Da.Item();
            items[0].ItemName = itemName;

            items = subscription.AddItems(items);

            subscription.DataChanged += new Opc.Da.DataChangedEventHandler(DataChange);
        }

        /// <summary>
        /// Вспомогательный метод для подписок. Вызывается при срабатывании подписки
        /// </summary>
        /// <param name="group"></param>
        /// <param name="hReq"></param>
        /// <param name="items"></param>
        void DataChange(object group, object hReq, Opc.Da.ItemValueResult[] items)
        {
            Console.WriteLine("-----------------");
            for (int i = 0; i < items.GetLength(0); i++)
            {
                Console.WriteLine("Item DataChange - ItemId: {0}", items[i].ItemName);
                Console.WriteLine("Value: {0,-20}", items[i].Value);
                //Console.WriteLine(" TimeStamp: {0:00}:{1:00}:{2:00}.{3:000}",
                //items[i].Timestamp.Hour,
                //items[i].Timestamp.Minute,
                //items[i].Timestamp.Second,
                //items[i].Timestamp.Millisecond);
                Console.WriteLine("TimeStamp: {0}", items[i].Timestamp.Date + items[i].Timestamp.TimeOfDay);

                Console.WriteLine("Sending request to controller");
                SendItemValueRequest(items[i]);
            }
            Console.WriteLine("-----------------");
            Thread.Sleep(3000);
        }

        /// <summary>
        /// Метод для поиска всех тегов на ОРС сервере
        /// </summary>
        /// <param name="itemName"></param>
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

        /// <summary>
        /// Вспомогательный метод для инициализации списка тегов с сервера.
        /// Вызывается при соединении с сервером
        /// </summary>
        /// <param name="itemName"></param>
        void InitTagList(string itemName = "")
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
                        var item = new Opc.Da.Item
                        {
                            ItemName = element.ItemName
                        };
                        tagList.Add(item);
                    }
                    else
                    {
                        InitTagList(element.ItemName);
                    }
                }
            }
        }

        public void UnsubscribeAll()
        {
            try
            {
                serverHandle.CancelSubscription(subscription);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error in Unsubscribing  {0}", exception);
            }
        }

        private void SendItemValueRequest(Opc.Da.ItemValue item)
        {
            // Генерация URL в котором производится сохранение параметра
            string url = "http://" + config.amicumIp;
            if (config.amicumPort != "")
            {
                url += ":" + config.amicumPort;
            }
            url += "/opc/handle-opc-data";

            Console.WriteLine(url);

            // Отправка данных на контроллер OPC
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            // Данные для отправки
            string data = "itemName=" + item.ItemName;
            data += "&itemValue=" + item.Value.ToString();
            data += "&dateTime=" + item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "." + item.Timestamp.Millisecond;
            //data += "&dateTime=" + (item.Timestamp.Date + item.Timestamp.TimeOfDay) + "." + item.Timestamp.Millisecond;
            data += "&connectString=" + serverHandle.Name;
            Console.WriteLine(data);
            // Преобразуем данные в массив байтов
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            // Устанавливаем тип содержимого - параметр ContentType
            request.ContentType = "application/x-www-form-urlencoded";
            // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
            request.ContentLength = byteArray.Length;

            // Записываем данные в поток запроса
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            // Получение ответа от сервера
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            response.Close();
        }

        /// <summary>
        /// Метод для записи значений тегов на сервер.
        /// Обращается к контроллеру для получения из кеша массива тегов и их
        /// значений, затем записывает все теги на сервер.
        /// </summary>
        public void GetWriteValueFromController()
        {
            if (!serverHandle.IsConnected)
            {
                Console.WriteLine("Нет подключения к серверу ОРС");
                return;
            }

            // Генерация URL в котором производится сохранение параметра
            string url = "http://" + config.amicumIp;
            if (config.amicumPort != "")
            {
                url += ":" + config.amicumPort;
            }
            url += "/opc/get-opc-write-value";

            Console.WriteLine(url);

            // Запрос к контроллеру на получение тегов для записи с последующей записью значений
            using (var webClient = new WebClient())
            {
                var response = webClient.DownloadString(url);
                Console.WriteLine(response);
                try
                {
                    var writeValues = JsonConvert.DeserializeObject<List<WriteItem>>(response);
                    foreach (var item in writeValues)
                    {
                        Write(item.itemName, item.itemValue);
                    }
                } catch (Exception exception)
                {
                    // Нужно будет убрать в финале, либо придумать другую обработку
                    Console.WriteLine("Кеш тегов для записи пуст. Исключение:");
                    Console.WriteLine(exception);
                }
            }
        }

    }
}
