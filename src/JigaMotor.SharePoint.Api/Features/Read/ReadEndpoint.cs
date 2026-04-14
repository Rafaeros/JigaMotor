namespace JigaMotor.SharePoint.Api.Features.Read;

public static class ReadEndpoint
{
    public static void MapReadSharePoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sharepoint/ler", async (ReadUseCase useCase) =>
        {
            var resultado = await useCase.GetAllAsync();
            return Results.Ok(resultado);
        })
        .WithName("LerSharePoint")
        .WithTags("SharePoint");
    }
}