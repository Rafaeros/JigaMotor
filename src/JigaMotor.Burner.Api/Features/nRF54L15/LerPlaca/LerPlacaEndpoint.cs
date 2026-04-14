using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Features.nRF54L15.LerPlaca;

public static class LerPlacaEndpoint
{
    public static void MapLerPlaca(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ler_placa", async (LerPlacaUseCase useCase) =>
        {
            try
            {
                var resultado = await useCase.ExecutarAsync();
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                // Devolve um erro 423 (Locked) se estiver protegido, ou 500 se o J-Link caiu.
                int statusCode = ex.Message.Contains("trancado") ? StatusCodes.Status423Locked : StatusCodes.Status500InternalServerError;
                return Results.Problem(detail: ex.Message, statusCode: statusCode);
            }
        })
        .WithName("LerPlaca")
        .WithTags("Manufatura - Jiga de Testes")
        .WithSummary("Lê o FICR de fábrica e as chaves LoRaWAN gravadas na RRAM")
        .WithDescription("ATENÇÃO: Este endpoint falhará (Retorno 423) se a placa estiver rodando o firmware Zephyr devido às travas de segurança de hardware (APPROT).")
        .Produces<LerPlacaResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status423Locked)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}