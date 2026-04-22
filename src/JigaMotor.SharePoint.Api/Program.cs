using JigaMotor.SharePoint.Api.Extensions;
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

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddOpenApi();
builder.Services.AddSharepointInfraestructure(builder.Configuration);
builder.Services.AddProblemDetails();
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("JigaMotor - SharePoint API")
               .WithTheme(ScalarTheme.DeepSpace);
    });
}

var apiV1 = app.MapGroup("/api/v1");

var devicesGroup = apiV1.MapGroup("/devices").WithTags("Devices");
devicesGroup.MapGetAllDevices();
devicesGroup.MapCheckDevEuiExists();
devicesGroup.MapGetDeviceByDevEui();
devicesGroup.MapCheckDeviceAvailability();
devicesGroup.MapCreateInitialDevice();
devicesGroup.MapUpdateHardwareTests();
devicesGroup.MapUpdatePowerConsumptionTests();
devicesGroup.MapUpdateProductionMetadata();
devicesGroup.MapDeleteDeviceByDevEui();
devicesGroup.MapAttachDeviceLog();

app.Run();