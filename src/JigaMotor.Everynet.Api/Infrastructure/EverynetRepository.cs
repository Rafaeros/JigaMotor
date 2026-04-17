using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;
using JigaMotor.Everynet.Api.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JigaMotor.Everynet.Api.Infrastructure
{
    public class EverynetRepository(HttpClient httpClient) : IEverynetRepository
    {

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public async Task<EverynetDevice?> GetDeviceByDevEuiAsync(string devEui)
        {
            var response = await httpClient.GetAsync($"devices/{devEui}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet:{response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();

            var wrapper = JsonSerializer.Deserialize<EverynetDeviceWrapper>(content, _jsonOptions);
            return wrapper?.Device;
        }

        public async Task<string> QueueDownlinkAsync(DownlinkCommand command)
        {
            var payloadObj = new
            {
                payload = command.Payload,
                port = command.Port,
                confirmed = command.Confirmed
            };

            var response = await httpClient.PostAsJsonAsync($"devices/{command.DevEui}/downlink", payloadObj, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new EverynetIntegrationException($"Erro na everynet:{response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private class EverynetDeviceWrapper
        {
            public EverynetDevice Device { get; set; } = null!;
        }
    }
}
