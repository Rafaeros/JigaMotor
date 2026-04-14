using JigaMotor.SharePoint.Api.Infraestructure;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Features.Read;
using JigaMotor.Shared.Config;
using JigaMotor.Shared.Interfaces;


namespace JigaMotor.SharePoint.Api.Extensions
{
    public static class SharepointConfiguration
    {

        public static IServiceCollection AddSharepointInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SharePointOptions>(configuration.GetSection("Sharepoint"));

            services.AddSingleton<GraphClientProvider>();

            services.AddScoped<IDeviceRepository, SharePointDeviceRepository>();

            services.AddScoped<ReadUseCase>();

            return services;
        }

    }
}
