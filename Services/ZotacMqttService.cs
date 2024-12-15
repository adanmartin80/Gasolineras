using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gasolineras.Services
{
    public class ZotacMqttService(MqttFactory mqttFactory, MqttClientOptionsBuilder mqttClientOptions, ILogger<ZotacMqttService> logger) : IDisposable
    {
        private int _tryConnectionCount = 1;
        private readonly IMqttClient _mqttClient = mqttFactory.CreateMqttClient();
        public CancellationToken CancellationToken { get; set; } = new();


        private async Task Connect()
        {

            try
            {
                if (_mqttClient.IsConnected) return;
                using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                {
                    var options = mqttClientOptions.Build();
                    var response = await _mqttClient.ConnectAsync(options, timeoutToken.Token);
                    logger.LogInformation("Cliente ZotacMQTT conectado.");

                    Reconnect_Using_Event();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"Timeout mientras se intentava la conexión {_tryConnectionCount}.");
                if (_tryConnectionCount <= 10) await Connect();
                else throw;
            }
            finally
            {
                _tryConnectionCount++;
            }

        }

        private void Reconnect_Using_Event()
        {
            _mqttClient.DisconnectedAsync += async e =>
            {
                if (e.ClientWasConnected && !CancellationToken.IsCancellationRequested)
                {
                    // Use the current options as the new options.
                    await _mqttClient.ReconnectAsync(CancellationToken);
                }
            };
        }

        public async Task Subscribe<T>(string topic, Action<T> action)
            where T : new()
        {
            await Connect();

            if (!_mqttClient.IsConnected) return;
            var response = await _mqttClient.SubscribeAsync(topic);

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                if (e.ApplicationMessage.Topic != topic) return;
                if (action == null || CancellationToken.IsCancellationRequested) return;
                if (string.IsNullOrEmpty(e.ApplicationMessage.ConvertPayloadToString())) return;
                var json = e.ApplicationMessage.ConvertPayloadToString();
                var objetData = JsonSerializer.Deserialize<T>(json) ?? new();
                await Task.Run(() => action(objetData));
            };

        }

        public async Task Publish<T>(string topic, T payload)
            where T : new()
        {
            await Connect();

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonSerializer.Serialize(payload))
                .Build();

            await _mqttClient.PublishAsync(applicationMessage, CancellationToken);
        }


        public void Dispose()
        {
            CancellationToken.ThrowIfCancellationRequested();
            _mqttClient.TryDisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection);
            _mqttClient?.Dispose();
        }
    }

    public static class PublishTopics
    {
        public static class Zotac
        {
            /// <summary>
            /// Usado para publicar el topic en el MQTT del Zotac.
            /// </summary>
            public static string CheapestPrice { get; private set; } = "homeassistant/petrolstation/cheapestprice";
            public static string All { get; private set; } = "homeassistant/petrolstation/all";
        }
    }

}
