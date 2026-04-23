using FluentValidation;
using JigaMotor.Everynet.Api.Extensions;
using JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff;
using JigaMotor.Everynet.Api.Features.Devices.ListDevices;
using JigaMotor.Everynet.Api.Features.Devices.CreateDevice;
using JigaMotor.Everynet.Api.Features.Devices.UpdateDevice;
using JigaMotor.Everynet.Api.Features.Devices.DeleteDevice;
using Scalar.AspNetCore;
using JigaMotor.Everynet.Api.Infrastructure.Serialization;
using JigaMotor.Everynet.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddOpenApi();
builder.Services.AddEverynetInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    options.SerializerOptions.Converters.Add(new EmptyStringToNullableIntConverter());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("JigaMotor - Everynet API")
               .WithTheme(ScalarTheme.DeepSpace);
    });
}

var apiV1 = app.MapGroup("/api/v1");

var devicesGroup = apiV1.MapGroup("/devices");
devicesGroup.MapGetDeviceByDevEuiEndpoint();
devicesGroup.MapSendEmergencyOnEndpoint();
devicesGroup.MapSendEmergencyOffEndpoint();
devicesGroup.MapListDevicesEndpoint();
devicesGroup.MapCreateDeviceEndpoint();
devicesGroup.MapUpdateDeviceEndpoint();
devicesGroup.MapDeleteDeviceEndpoint();

app.Run();
