using JigaMotor.SharePoint.Api.Extensions; // Para achar o seu Extension Method
using JigaMotor.SharePoint.Api.Features.Common; // Para o GraphClientProvider
using JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists;
using JigaMotor.SharePoint.Api.Features.Device.GetAllDevices;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// --- 1. SERVIÇOS (Usando a sua Injeção Limpa) ---
builder.Services.AddOpenApi();
builder.Services.AddSharepointInfraestructure(builder.Configuration);
var app = builder.Build();

// --- 2. AQUECIMENTO (WARM-UP) DA API ---
// Isso roda assim que você dá 'dotnet run', ANTES da API liberar a porta pro navegador
using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("⏳ [Startup] Iniciando autenticação no SharePoint...");
    try
    {
        // Pede o Provider pro .NET. Aqui ele já monta o IOptions pra você automaticamente!
        var graphProvider = scope.ServiceProvider.GetRequiredService<GraphClientProvider>();

        // Chama o método para forçar a criação do _graphClient e validar a credencial
        var graphClient = graphProvider.GetAuthenticatedClient();

        Console.WriteLine("✅ [Startup] SharePoint autenticado com sucesso e pronto para uso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ [Startup] Erro na autenticação: {ex.Message}");
    }
}

// --- 3. PIPELINE HTTP ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("JigaMotor - SharePoint API")
               .WithTheme(ScalarTheme.DeepSpace);
    });
}

app.MapGetAllDevices();
app.MapCheckDevEuiExists();

app.Run();