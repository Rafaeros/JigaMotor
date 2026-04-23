using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
                throw new EverynetIntegrationException($"Erro na everynet (Management API - Get): {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<EverynetDeviceWrapper>(content, _jsonOptions);
            return wrapper?.Device;
        }

        public async Task<EverynetDevice> CreateDeviceAsync(EverynetDevice device)
        {
            var json = JsonSerializer.Serialize(device, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("devices", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                
                if (response.StatusCode == HttpStatusCode.Forbidden && error.Contains("Device already existing"))
                {
                    throw new EverynetDeviceAlreadyExistsException(device.DevEui);
                }

                throw new EverynetIntegrationException($"Erro na everynet (Management API - Create): {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<EverynetDeviceWrapper>(responseContent, _jsonOptions);
            return wrapper?.Device ?? throw new EverynetIntegrationException("Erro ao deserializar resposta de criação de dispositivo.");
        }

        public async Task<(IEnumerable<EverynetDevice> Devices, int Total)> ListDevicesAsync(int limit, int offset, string? query)
        {
            var url = $"devices?limit={limit}&offset={offset}";
            if (!string.IsNullOrEmpty(query))
                url += $"&q={WebUtility.UrlEncode(query)}";

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet (Management API - List): {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<EverynetListWrapper>(content, _jsonOptions);
            return (wrapper?.Keys ?? [], wrapper?.Total ?? 0);
        }

        public async Task<EverynetDevice> UpdateDeviceAsync(string devEui, EverynetDevice device)
        {
            var wrapperRequest = new EverynetDeviceWrapper { Device = device };
            var json = JsonSerializer.Serialize(wrapperRequest, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PatchAsync($"devices/{devEui}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet (Management API - Update): {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<EverynetDeviceWrapper>(responseContent, _jsonOptions);
            return wrapper?.Device ?? throw new EverynetIntegrationException("Erro ao deserializar resposta de atualização de dispositivo.");
        }

        public async Task DeleteDeviceAsync(string devEui)
        {
            var response = await httpClient.DeleteAsync($"devices/{devEui}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet (Management API - Delete): {response.StatusCode} - {error}");
            }
        }


        public async Task<string> QueueDownlinkAsync(DownlinkCommand command)
        {
            var downlinkMessage = new EverynetDownlinkMessage
            {
                Meta = new EverynetMeta { Device = command.DevEui },
                Params = new EverynetParams
                {
                    Port = command.Port,
                    Payload = command.Payload,
                    Confirmed = command.Confirmed
                }
            };

            var jsonPayload = JsonSerializer.Serialize(downlinkMessage, _jsonOptions);

            var mqttServer = configuration["Everynet:Mqtt:Server"] ?? throw new InvalidOperationException("Servidor MQTT não configurado no appsettings.");
            var mqttPort = int.Parse(configuration["Everynet:Mqtt:Port"] ?? "1883");
            var mqttUser = configuration["Everynet:Mqtt:User"];
            var mqttPass = configuration["Everynet:Mqtt:Password"];
            var pubTopic = configuration["Everynet:Mqtt:PublishTopic"] ?? "iot/sub";

            var mqttFactory = new MqttFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttOptions = new MqttClientOptionsBuilder()
                          .WithTcpServer(mqttServer, mqttPort)
                          .WithCredentials(mqttUser, mqttPass)
                          .WithClientId($"JigaMotor_{Guid.NewGuid()}")
                          .WithTlsOptions(o => o.UseTls())
                          .Build();

            try
            {
                await mqttClient.ConnectAsync(mqttOptions);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(pubTopic)
                    .WithPayload(jsonPayload)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage);
                await mqttClient.DisconnectAsync();

                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                throw new EverynetIntegrationException($"Erro ao publicar comando no MQTT da Everynet: {ex.Message}");
            }
        }


        private class EverynetDeviceWrapper
        {
            public EverynetDevice Device { get; set; } = null!;
        }

        private class EverynetListWrapper
        {
            public int Total { get; set; }
            public int Limit { get; set; }
            public int Offset { get; set; }
            public IEnumerable<EverynetDevice> Keys { get; set; } = null!;
        }

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