using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using BlazorApp1.Data;
using DataSet = BlazorApp1.Data.DataSet;

namespace BlazorApp1.Services
{
    public class MqttService
    {
        public async Task Send_Responses()
        {
            /*
             * This sample subscribes to a topic and sends a response to the broker. This requires at least QoS level 1 to work!
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                mqttClient.ApplicationMessageReceivedAsync += delegate (MqttApplicationMessageReceivedEventArgs args)
                {
                    // Do some work with the message...

                    // Now respond to the broker with a reason code other than success.
                    args.ReasonCode = MqttApplicationMessageReceivedReasonCode.ImplementationSpecificError;
                    args.ResponseReasonString = "That did not work!";

                    // User properties require MQTT v5!
                    args.ResponseUserProperties.Add(new MqttUserProperty("My", "Data"));

                    // Now the broker will resend the message again.
                    return Task.CompletedTask;
                };

                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic("mqttnet/samples/topic/1");
                        })
                    .Build();

                var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                // The response contains additional data sent by the server after subscribing.
                //response.DumpToConsole();
            }
        }

        public async Task Subscribe_Topic(DataSet dataset)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId(dataset.MQTT_CLIENT_ID)
                        .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                        .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                        .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(dataset.MQTT_TOPIC_SUB);
                        })
                    .Build();

                var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                // The response contains additional data sent by the server after subscribing.
                //response.DumpToConsole();
            }
        }

        public async Task Publish_Application_Message(DataSet dataset, DataGet dataGet)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                            .WithClientId(dataset.MQTT_CLIENT_ID)
                            .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                            .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                            .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(dataset.MQTT_TOPIC_PUB)
                    .WithPayload(dataGet.Temperature)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }
        
        public async Task<bool> Clean_Disconnect(DataSet dataset)
        {
            try
            {
                var mqttFactory = new MqttFactory();

                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                            .WithClientId(dataset.MQTT_CLIENT_ID)
                            .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                            .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                            .Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> Connect_Client(DataSet dataset)
        {
            try
            {
                var mqttFactory = new MqttFactory();

                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    // Use builder classes where possible in this project.
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId(dataset.MQTT_CLIENT_ID)
                        .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                        .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                        .Build();

                    // The result from this message returns additional data which was sent 
                    MqttClientConnectResult response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    return response.ResultCode == 0;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task Ping_Server(DataSet dataset)
        {
            /*
             * This sample sends a PINGREQ packet to the server and waits for a reply.
             *
             * This is only supported in MQTTv5.0.0+.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithClientId(dataset.MQTT_CLIENT_ID)
                    .WithTcpServer(dataset.MQTT_BROKER, Convert.ToInt32(dataset.MQTT_PORT))
                    .WithCredentials(dataset.MQTT_USER, dataset.MQTT_PASSWORD)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                // This will throw an exception if the server does not reply.
                await mqttClient.PingAsync(CancellationToken.None);

                Console.WriteLine("The MQTT server replied to the ping request.");
            }
        }

    }
}
