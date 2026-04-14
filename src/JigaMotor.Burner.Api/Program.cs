using JigaMotor.Features.nRF54L15.GravarPlaca;
using JigaMotor.Features.nRF54L15.LerPlaca;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Injeção de Dependência dos UseCases
builder.Services.AddScoped<GravarPlacaUseCase>();
builder.Services.AddScoped<LerPlacaUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// O UseHttpsRedirection pode atrapalhar o acesso via IP na rede local se não houver certificado, 
// se der erro de conexão no celular, experimente comentar a linha abaixo:
// app.UseHttpsRedirection();

app.MapGravarPlaca();
app.MapLerPlaca();

app.Run();