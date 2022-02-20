using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace KiwiBot
{
    public class Kiwi
    {
        private const string _rabbitMQUrl = "amqps://pnceccsp:BFnfl8mUyG67F5oILrS6Z9PX5rfqdcDN@woodpecker.rmq.cloudamqp.com/pnceccsp";

        private const string _stockQuoteUrl = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e=csv";
        private const string _stockQuoteMessage = "{0} quote is ${1} per share!.";

        private readonly string _readQueueName;
        private readonly string _writeQueueName;

        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _readChannel;
        private readonly IModel _writeChannel;

        public Kiwi(bool productionEnvironment)
        {
            if (productionEnvironment)
            {
                _readQueueName = "kiwi";
                _writeQueueName = "talksity";
            }
            else
            {
                _readQueueName = "kiwiTest";
                _writeQueueName = "talksityTest";
            }

            // initialization for the connection wit RabbitMQ 
            _factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
            _connection = _factory.CreateConnection();

            // read channel 
            _readChannel = _connection.CreateModel();

            _readChannel.QueueDeclare(queue: _readQueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            // write channel
            _writeChannel = _connection.CreateModel();

            _writeChannel.QueueDeclare(queue: _writeQueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
        }

        //Read queue of message from chat application (talksity)
        public bool ReadQueue()
        {
            var consumer = new EventingBasicConsumer(_readChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Kiwi tries to understand the message
                AnalizeCommand(message);
            };

            try
            {
                _readChannel.BasicConsume(queue: _readQueueName,
                                    autoAck: true,
                                    consumer: consumer);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void AnalizeCommand(string message)
        {
            //get the command
            string command = message.Split('=')[0].ToLower();

            switch (command)
            {
                case "/stock":
                    if (message.Split('=').Length >= 1)
                    {
                        //get the parameter
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

        public decimal? GetStockQuote(string parameter)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(_stockQuoteUrl, parameter));
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //read the response as a memorystream
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string results = sr.ReadToEnd();

                    decimal? quote = GetQuote(results);

                    if (quote.HasValue)
                    {
                        WriteQueue(string.Format(_stockQuoteMessage, parameter.ToUpper(), quote));
                        return quote;
                    }
                    else
                    {
                        WriteQueue("quote not found");
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public decimal? GetQuote(string results)
        {
            if (string.IsNullOrWhiteSpace(results)) return null;

            //separate the file into lines
            string[] lines = results.Split(Environment.NewLine);

            if(lines.Length < 2) return null;

            int columnLength = lines[0].Split(',').Length;

            // get the first line as headers
            string[] headers = lines[0].Split(',');

            // get the second line as the content
            string[] data = lines[1].Split(',');

            Dictionary<string, string> dictionaryData = new Dictionary<string, string>();

            for (int i = 0; i < columnLength; i++)
            {
                //saves the information as header/data dictionary
                dictionaryData.Add(headers[i], data[i]);
            }

            //try to parse the data as decimal
            if (decimal.TryParse(dictionaryData["Open"], NumberStyles.Number, new CultureInfo("en-US"), out decimal quote))
            {
                return quote;
            }
            else
            {
                return null;
            }
        }

        //Send message back to another queue to be read by the chat application (talksity)
        public bool WriteQueue(string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);

                _writeChannel.BasicPublish(exchange: "",
                                            routingKey: _writeQueueName,
                                            basicProperties: null,
                                            body: body);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
