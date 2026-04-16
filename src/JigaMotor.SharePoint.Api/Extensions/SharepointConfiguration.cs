using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using JigaMotor.SharePoint.Api.Domain.Interfaces;
using JigaMotor.SharePoint.Api.Infrastructure;
using JigaMotor.SharePoint.Api.Features.Device.GetAllDevices;
using JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists;


namespace JigaMotor.SharePoint.Api.Extensions
{
    public static class SharepointConfiguration
    {

        public static IServiceCollection AddSharepointInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SharePointOptions>(configuration.GetSection("Sharepoint"));

            services.AddSingleton<GraphClientProvider>();

            services.AddScoped<IDeviceRepository, SharePointDeviceRepository>();

            services.AddScoped<GetAllDevicesUseCase>();
            services.AddScoped<CheckDevEuiExistsUseCase>();

            return services;
        }

    }
}
