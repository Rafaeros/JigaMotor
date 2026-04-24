using JigaMotor.SharePoint.Api.Domain.Interfaces;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists;
using JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;
using JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.SharePoint.Api.Features.Devices.CheckDeviceAvailability;
using JigaMotor.SharePoint.Api.Features.Devices.CreateInitialDevice;
using JigaMotor.SharePoint.Api.Features.Devices.UpdateHardwareTests;
using JigaMotor.SharePoint.Api.Features.Devices.UpdateProductionMetadata;
using JigaMotor.SharePoint.Api.Features.Devices.DeleteDeviceByDevEui;
using JigaMotor.SharePoint.Api.Features.Devices.AttachDeviceLog;
using JigaMotor.SharePoint.Api.Features.Devices.UpdatePowerConsumptionTests;
using FluentValidation;
using JigaMotor.SharePoint.Api.Infrastructure;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using JigaMotor.SharePoint.Api.Features.Firmwares.GetFirmwaresByModel;


namespace JigaMotor.SharePoint.Api.Extensions
{
    public static class SharepointConfiguration
    {

        public static IServiceCollection AddSharepointInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.Configure<SharePointOptions>(configuration.GetSection("Sharepoint"));

            services.AddSingleton<GraphClientProvider>();

            services.AddScoped<IDeviceRepository, SharePointDeviceRepository>();
            services.AddScoped<IFirmwareRepository, SharePointFirmwareRepository>();

            services.AddScoped<GetAllDevicesUseCase>();
            services.AddScoped<CheckDevEuiExistsUseCase>();
            services.AddScoped<GetDeviceByDevEuiUseCase>();
            services.AddScoped<CheckDeviceAvailabilityUseCase>();
            services.AddScoped<CreateInitialDeviceUseCase>();
            services.AddScoped<UpdateHardwareTestsUseCase>();
            services.AddScoped<UpdateProductionMetadataUseCase>();
            services.AddScoped<DeleteDeviceByDevEuiUseCase>();
            services.AddScoped<AttachDeviceLogUseCase>();
            services.AddScoped<UpdatePowerConsumptionTestsUseCase>();
            services.AddScoped<GetFirmwaresByModelUseCase>();

            services.AddValidatorsFromAssembly(typeof(SharepointConfiguration).Assembly);

            return services;
        }

    }
}
