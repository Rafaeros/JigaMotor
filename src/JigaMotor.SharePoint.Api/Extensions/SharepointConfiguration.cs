using JigaMotor.SharePoint.Api.Domain.Interfaces;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists;
using JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;
using JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.SharePoint.Api.Infrastructure;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;


namespace JigaMotor.SharePoint.Api.Extensions
{
    public static class SharepointConfiguration
    {

        public static IServiceCollection AddSharepointInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SharePointOptions>(configuration.GetSection("Sharepoint"));

            services.AddSingleton<GraphClientProvider>();

            services.AddScoped<IDeviceRepository, SharePointDeviceRepository>();

            // Device Use Cases
            services.AddScoped<GetAllDevicesUseCase>();
            services.AddScoped<CheckDevEuiExistsUseCase>();
            services.AddScoped<GetDeviceByDevEuiUseCase>();

            return services;
        }

    }
}
