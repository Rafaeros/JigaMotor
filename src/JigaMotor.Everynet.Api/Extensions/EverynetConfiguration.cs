using JigaMotor.Everynet.Api.Domain.Interfaces;
using JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.Everynet.Api.Infrastructure;
using JigaMotor.Everynet.Api.Infrastructure.Configuration;
using JigaMotor.Everynet.Api.Infrastructure.Handlers;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff;

namespace JigaMotor.Everynet.Api.Extensions
{
    public static class EverynetConfiguration
    {
        public static IServiceCollection AddEverynetInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EverynetOptions>(configuration.GetSection("Everynet"));
            var options = configuration.GetSection("Everynet").Get<EverynetOptions>()
                ?? throw new InvalidOperationException("A configuração 'Everynet' não foi encontrada no appsettings.json.");

            services.AddTransient<EverynetAuthHandler>();

            services.AddHttpClient<IEverynetRepository, EverynetRepository>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
                    .AddHttpMessageHandler<EverynetAuthHandler>();

            services.AddScoped<GetDeviceByDevEuiUseCase>();
            services.AddScoped<SendEmergencyOnUseCase>();
            services.AddScoped<SendEmergencyOffUseCase>();

            return services;    
        }
    }
}
