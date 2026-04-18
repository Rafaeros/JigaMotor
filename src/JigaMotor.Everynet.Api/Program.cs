using FluentValidation;
using JigaMotor.Everynet.Api.Extensions;
using JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn;
using JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddOpenApi();
builder.Services.AddEverynetInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();

var app = builder.Build();

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

var devicesGroup = apiV1.MapGroup("/devices").WithTags("Devices");
devicesGroup.MapGetDeviceByDevEuiEndpoint();
devicesGroup.MapSendEmergencyOnEndpoint();
devicesGroup.MapSendEmergencyOffEndpoint();

app.Run();
