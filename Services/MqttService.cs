using MQTTnet;
using MQTTnet.Client;
using BlazorApp1.Data;
using DataSet = BlazorApp1.Data.DataSet;
using MQTTnet.Server;
using MQTTnet.Client.Options;
using System.Text;
using System.Data;
using MQTTnet.Client.Publishing;
using System.Text.Json;
using Newtonsoft.Json;

namespace BlazorApp1.Services
{
    public class MqttService
    {
        DataGet dataGet = new();

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

                Console.WriteLine("Connected to the broker successfully");
                client.UseApplicationMessageReceivedHandler(args =>
                {
                    dataGet.Temperature = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                    Console.WriteLine($"Received Message - {Encoding.UTF8.GetString(args.ApplicationMessage.Payload)}");
                });

                await client.ConnectAsync(options);

                //await client.DisconnectAsync();

                return dataGet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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

                if (entryGet.Temperature != null && entryGet.Temperature != "0")
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
                    valor = entryGet.Temperature
                });

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(dataset.MQTT_TOPIC_PUB)
                    .WithPayload(objectSerialized)
                    .WithAtLeastOnceQoS()
                    .Build();

                if (client.IsConnected)
                {
                    await client.PublishAsync(message);
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
