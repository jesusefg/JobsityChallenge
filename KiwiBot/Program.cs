using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KiwiBot
{
    internal class Program
    {
        private const int _sleepTime = 1000 * 3; // 3 seconds
        private const string _rabbitMQUrl = "amqps://pnceccsp:BFnfl8mUyG67F5oILrS6Z9PX5rfqdcDN@woodpecker.rmq.cloudamqp.com/pnceccsp";
        private const string _readQueueName = "kiwi";
        private const string _writeQueueName = "talksity";

        private const string _stockQuoteUrl = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e=csv";
        private const string _stockQuoteMessage = "{0} quote is ${1} per share!.";

        static void Main(string[] args)
        {
            do
            {
                ReadQueue();
                Thread.Sleep(_sleepTime);
            } while (true);
        }

        public static void ReadQueue()
        {
            
                var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _readQueueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Message received -> " + message);
                        // Kiwi tries to understand the message
                        AnalizeCommand(message);
                    };
                    channel.BasicConsume(queue: _readQueueName,
                                         autoAck: true,
                                         consumer: consumer);
                }
        }

        static void AnalizeCommand(string message)
        {
            string command = message.Split('=')[0].ToLower();

            switch (command)
            {
                case "/stock":
                    if(message.Split('=').Length >= 1)
                    {
                        string parameter = message.Split('=')[1];
                        GetStockQuote(parameter);
                    }
                    else
                    {
                        WriteQueue(string.Format("Missing parameter for command {0}", command));
                    }

                    break;
                default:
                    WriteQueue("Unknown command!");
                    break;
            }
        }

        static void GetStockQuote(string parameter)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(_stockQuoteUrl,parameter));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader sr = new StreamReader(response.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            decimal? quote = GetQuote(results);

            if (quote.HasValue)
            {
                WriteQueue(string.Format(_stockQuoteMessage, parameter.ToUpper(), quote));
            }
            else
            {
                WriteQueue("quote not found");
            }            
        }

        public static decimal? GetQuote(string results)
        {
            string[] lines = results.Split(Environment.NewLine);

            int columnLength = lines[0].Split(',').Length;
            string[] headers = lines[0].Split(',');
            string[] data = lines[1].Split(',');

            Dictionary<string,string> dictionaryData = new Dictionary<string,string>();

            for(int i = 0; i < columnLength; i++)
            {
                dictionaryData.Add(headers[i], data[i]);
            }

            if(decimal.TryParse(dictionaryData["Open"], NumberStyles.Number, new CultureInfo("en-US"), out decimal quote))
            {
                return quote;
            }
            else
            {
                return null;
            }
        }

        static void WriteQueue(string message)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _writeQueueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                        routingKey: _writeQueueName,
                                        basicProperties: null,
                                        body: body);
            }
        }
    }
}
