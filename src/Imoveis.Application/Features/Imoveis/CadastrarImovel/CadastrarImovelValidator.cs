using FluentValidation;

namespace Imoveis.Application.Features.Imoveis.CadastrarImovel;

public class CadastrarImovelValidator : AbstractValidator<CadastrarImovelCommand>
{
    public CadastrarImovelValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve ter exatamente 2 caracteres (ex: SP, RJ).");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleFor(x => x.AreaM2)
            .GreaterThan(0).WithMessage("Área deve ser maior que zero.");

        RuleFor(x => x.Quartos)
            .GreaterThanOrEqualTo(0).WithMessage("Número de quartos não pode ser negativo.");
    }
}
