using JigaMotor.SharePoint.Api.Extensions;
using JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists;
using JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;
using JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui;

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
apiV1.MapGetAllDevices();
apiV1.MapCheckDevEuiExists();
apiV1.MapGetDeviceByDevEui();

app.Run();