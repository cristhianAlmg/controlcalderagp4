using MQTTnet;
using MQTTnet.Client;
using BlazorApp1.Data;
using DataSet = BlazorApp1.Data.DataSet;
using MQTTnet.Server;
using MQTTnet.Client.Options;
using System.Text;
using Newtonsoft.Json;

namespace BlazorApp1.Services
{
    public class MqttService
    {
        DataGet dataGet = new();
        string? result;

        public async Task<DataGet> Subscribe_Topic(DataSet dataset)
        {
            try
            {
                var mqttFactory = new MqttFactory();

                IMqttClient client = mqttFactory.CreateMqttClient();

                IMqttClientOptions options = new MqttClientOptionsBuilder()
                    .WithClientId(dataset.MQTT_CLIENT_ID)
                    .WithTcpServer(dataset.MQTT_BROKER, dataset.MQTT_PORT)
                    .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                    .WithCleanSession()
                    .Build();

                client.UseConnectedHandler(args =>
                {
                    var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic(dataset.MQTT_TOPIC_SUB)
                    .WithAtLeastOnceQoS()
                    .Build();
                    
                    client.SubscribeAsync(topicFilter);                    
                });

                client.UseDisconnectedHandler(args => { });

                client.UseApplicationMessageReceivedHandler(args =>
                {
                    Console.WriteLine("Connected to the broker successfully");
                    result = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                    Console.WriteLine($"Received Message - {Encoding.UTF8.GetString(args.ApplicationMessage.Payload)}");
                });

                dataGet = ValidateResult(result: result);

                await client.ConnectAsync(options);

                return dataGet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private static DataGet ValidateResult(string? result)
        {
            return (!string.IsNullOrEmpty(result)) ? FormatedResult(result) : new DataGet();
        }

        private static DataGet FormatedResult(string result)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataGet>(result);
            }
            catch (Exception)
            {
                return new DataGet();
            }
        }

        public async Task<bool> Publish_Application_Message(DataSet dataset, DataGet entryGet)
        {    
            try
            {
                var mqttFactory = new MqttFactory();

                IMqttClient client = mqttFactory.CreateMqttClient();

                IMqttClientOptions options = new MqttClientOptionsBuilder()
                    .WithClientId(dataset.MQTT_CLIENT_ID)
                    .WithTcpServer(dataset.MQTT_BROKER, dataset.MQTT_PORT)
                    .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                    .WithCleanSession()
                    .Build();

                client.UseConnectedHandler(args => { });

                client.UseDisconnectedHandler(args => { });

                await client.ConnectAsync(options);

                if ((!string.IsNullOrEmpty(entryGet.Temperatura)))
                {
                    await PublishMessageAsync(client, dataset, entryGet);
                }

                await client.DisconnectAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static async Task PublishMessageAsync(IMqttClient client, DataSet dataset, DataGet entryGet)
        {
            try
            {
                string objectSerialized = JsonConvert.SerializeObject(new
                {
                    tipo = dataset.MQTT_ACTUADOR,
                    valor = entryGet.Temperatura
                });

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(dataset.MQTT_TOPIC_PUB)
                    .WithPayload(objectSerialized)
                    .WithAtLeastOnceQoS()
                    .Build();

                if (client.IsConnected)
                {
                    Console.WriteLine("Connected to the broker successfully");
                    await client.PublishAsync(message);
                    Console.WriteLine($"Published Message - {objectSerialized}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
