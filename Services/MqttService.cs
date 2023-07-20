using MQTTnet;
using MQTTnet.Client;
using BlazorApp1.Data;
using DataSet = BlazorApp1.Data.DataSet;
using MQTTnet.Server;
using MQTTnet.Client.Options;
using System.Text;
using System.Data;

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

                var options = new MqttClientOptionsBuilder()
                    .WithClientId(dataset.MQTT_CLIENT_ID)
                    .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                    .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                    .Build();

                var topicFilter = new MqttTopicFilterBuilder().WithTopic(dataset.MQTT_TOPIC_SUB).Build();

                await client.SubscribeAsync(topicFilter);

                client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("Connected to the broker successfully");
                });

                client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("Disconnected from the broker successfull");
                });

                client.UseApplicationMessageReceivedHandler(e =>
                {
                    dataGet.Temperature = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    Console.WriteLine($"Received Message - {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                });

                await client.ConnectAsync(options);

                await client.DisconnectAsync();

                return dataGet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<bool> Publish_Application_Message(DataSet dataset, DataGet dataget)
        {    
            try
            {
                var mqttFactory = new MqttFactory();

                IMqttClient client = mqttFactory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithClientId(dataset.MQTT_CLIENT_ID)
                    .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                    .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                    .Build();

                client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("Connected to the broker successfully");
                });

                client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("Disconnected from the broker successfull");
                });

                await client.ConnectAsync(options);

                await PublishMessageAsync(client, dataset, dataget);

                await client.DisconnectAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static async Task PublishMessageAsync(IMqttClient client, DataSet dataset, DataGet dataget)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(dataset.MQTT_TOPIC_SUB)
                    .WithPayload(dataget.Temperature)
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
