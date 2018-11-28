using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OPCClientCSTest
{
    /// <summary>
    /// Класс, содержащий настройки из конфигурационного файла
    /// </summary>
    class OpcClientConfig
    {
        public string amicumIp = "";
        public string opcServerId = "";
        public string amicumPort = "";

        /// <summary>
        /// Метод, выполняющий чтение настроек из конфигурационного файла
        /// </summary>
        public void GetConfig()
        {
            try
            {
                using (var file = new StreamReader("config.txt"))
                {
                    string tmpLine = "";
                    while ((tmpLine = file.ReadLine()) != null)
                    {
                        string[] configLine = tmpLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        switch (configLine[0])
                        {
                            case "AmicumIp":
                                amicumIp = configLine[1];
                                break;
                            case "OpcServerId":
                                opcServerId = configLine[1];
                                break;
                            case "AmicumPort":
                                amicumPort = configLine[1];
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error while reading config file - status {0}", exception.Message);
            }
        }
    }
}