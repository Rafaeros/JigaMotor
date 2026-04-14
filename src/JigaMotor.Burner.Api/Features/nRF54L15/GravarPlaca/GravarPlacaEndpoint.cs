namespace JigaMotor.Features.nRF54L15.GravarPlaca;

public static class GravarPlacaEndpoint
{
    public static void MapGravarPlaca(this IEndpointRouteBuilder app)
    {
        app.MapPost("/gravar_placa", async (GravarPlacaRequest request, GravarPlacaUseCase useCase) =>
        {
            try
            {
                var resultado = await useCase.ExecutarAsync(request);
                return Results.Ok(resultado);
            }
            catch (ArgumentException ex) // Tratamento para chaves em formato inválido
            {
                return Results.BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex) // Tratamento para cabos soltos ou erros do J-Link
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })

        // ==========================================================
        // CONFIGURAÇÕES VISUAIS E CONTRATOS PARA O SWAGGER
        // ==========================================================

        .WithName("GravarPlaca")
        .WithTags("Manufatura - Jiga de Testes") // Cria uma "pasta" visual no Swagger
        .WithSummary("Grava o firmware e injeta chaves LoRaWAN no nRF54L15")
        .WithDescription("Executa rotinas de baixo nível usando nrfjprog. Apaga o chip via Recover, injeta o DevEUI/AppEUI/AppKey na RRAM (0x0017E000) e faz o flash do SO.")

        // Documenta o que a API devolve em caso de Sucesso (Caixa Verde no Swagger)
        .Produces<GravarPlacaResponse>(StatusCodes.Status200OK)

        // Documenta o que a API devolve se o payload vier errado (Caixa Amarela no Swagger)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)

        // Documenta o que a API devolve se a Jiga falhar fisicamente (Caixa Vermelha no Swagger)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}