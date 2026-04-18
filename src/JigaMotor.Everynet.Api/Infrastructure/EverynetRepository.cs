using System.Net;
using System.Text.Json;
using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;
using JigaMotor.Everynet.Api.Exceptions;
using MQTTnet;
using MQTTnet.Client;

namespace JigaMotor.Everynet.Api.Infrastructure
{
    public class EverynetRepository(HttpClient httpClient, IConfiguration configuration) : IEverynetRepository
    {
        // Cache estático de opções do JSON para alta performance
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        // =================================================================
        // 1. MANAGEMENT API (HTTP GET - Lê informações do banco de dados)
        // =================================================================
        public async Task<EverynetDevice?> GetDeviceByDevEuiAsync(string devEui)
        {
            var response = await httpClient.GetAsync($"devices/{devEui}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet (Management API): {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<EverynetDeviceWrapper>(content, _jsonOptions);
            return wrapper?.Device;
        }


        // =================================================================
        // 2. DATA API (MQTT PUBLISH - Envia comandos de rádio/Downlink)
        // =================================================================
        public async Task<string> QueueDownlinkAsync(DownlinkCommand command)
        {
            // Monta a estrutura JSON exata que a Everynet exige no tópico
            var downlinkMessage = new EverynetDownlinkMessage
            {
                Meta = new EverynetMeta { Device = command.DevEui },
                Params = new EverynetParams
                {
                    Port = command.Port,
                    Payload = command.Payload, // O Base64 dinâmico que veio da Jiga
                    Confirmed = command.Confirmed
                }
            };

            var jsonPayload = JsonSerializer.Serialize(downlinkMessage, _jsonOptions);

            // Lê as configurações do appsettings.json
            var mqttServer = configuration["Everynet:Mqtt:Server"] ?? throw new InvalidOperationException("Servidor MQTT não configurado no appsettings.");
            var mqttPort = int.Parse(configuration["Everynet:Mqtt:Port"] ?? "1883");
            var mqttUser = configuration["Everynet:Mqtt:User"];
            var mqttPass = configuration["Everynet:Mqtt:Password"];
            var pubTopic = configuration["Everynet:Mqtt:PublishTopic"] ?? "iot/sub";

            // Instancia o cliente MQTT
            var mqttFactory = new MqttFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttOptions = new MqttClientOptionsBuilder()
                          .WithTcpServer(mqttServer, mqttPort)
                          .WithCredentials(mqttUser, mqttPass)
                          .WithClientId($"JigaMotor_{Guid.NewGuid()}")
                          .WithTlsOptions(o => o.UseTls()) // <--- ESTA É A CHAVE MÁGICA DO HIVEMQ!
                          .Build();

            try
            {
                // Conecta e dispara o pacote!
                await mqttClient.ConnectAsync(mqttOptions);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(pubTopic)
                    .WithPayload(jsonPayload)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage);
                await mqttClient.DisconnectAsync();

                // Como o MQTT é assíncrono e muito rápido, geramos um ID de rastreio próprio
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                throw new EverynetIntegrationException($"Erro ao publicar comando no MQTT da Everynet: {ex.Message}");
            }
        }


        // =================================================================
        // CLASSES AUXILIARES PRIVADAS (O segredo do encapsulamento)
        // =================================================================

        // Classe para ler o GET
        private class EverynetDeviceWrapper
        {
            public EverynetDevice Device { get; set; } = null!;
        }

        // Classes para montar o JSON do MQTT
        private class EverynetDownlinkMessage
        {
            public required EverynetMeta Meta { get; set; }
            public string Type { get; set; } = "downlink_response";
            public required EverynetParams Params { get; set; }
        }

        private class EverynetMeta
        {
            public required string Device { get; set; }
        }

        private class EverynetParams
        {
            public bool Confirmed { get; set; }
            public int Port { get; set; }
            public required string Payload { get; set; }
        }
    }
}