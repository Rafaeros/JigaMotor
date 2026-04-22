using FluentValidation;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateProductionMetadata
{
    public record UpdateProductionMetadataRequest(
        string Serie,
        string BoxNumber,
        string ProductionOrder,
        string TagStatus,
        string RdpStatus
    );

    public class UpdateProductionMetadataValidator : AbstractValidator<UpdateProductionMetadataRequest>
    {
        public UpdateProductionMetadataValidator()
        {
            RuleFor(x => x.Serie).NotEmpty().WithMessage("O Número de Série é obrigatório.");
            RuleFor(x => x.BoxNumber).NotEmpty().WithMessage("O Número da Caixa é obrigatório.");
            RuleFor(x => x.ProductionOrder).NotEmpty().WithMessage("A Ordem de Produção é obrigatória.");
            RuleFor(x => x.TagStatus).NotEmpty().WithMessage("O Status da Tag é obrigatório.");
            RuleFor(x => x.RdpStatus).NotEmpty().WithMessage("O Status do RDP é obrigatório.");
        }
    }
}
