using JigaMotor.Everynet.Api.Domain.Interfaces;
using JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.Everynet.Api.Infrastructure;
using JigaMotor.Everynet.Api.Infrastructure.Configuration;
using JigaMotor.Everynet.Api.Infrastructure.Handlers;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff;
using JigaMotor.Everynet.Api.Features.Devices.ListDevices;
using JigaMotor.Everynet.Api.Features.Devices.CreateDevice;
using JigaMotor.Everynet.Api.Features.Devices.UpdateDevice;
using JigaMotor.Everynet.Api.Features.Devices.DeleteDevice;

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
            services.AddScoped<ListDevicesUseCase>();
            services.AddScoped<CreateDeviceUseCase>();
            services.AddScoped<UpdateDeviceUseCase>();
            services.AddScoped<DeleteDeviceUseCase>();

            return services;    
        }
    }
}
