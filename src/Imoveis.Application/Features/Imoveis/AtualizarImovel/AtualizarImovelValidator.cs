using FluentValidation;

namespace Imoveis.Application.Features.Imoveis.AtualizarImovel;

public class AtualizarImovelValidator : AbstractValidator<AtualizarImovelCommand>
{
    public AtualizarImovelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório.")
            .Matches(@"^\d{8}$").WithMessage("CEP deve conter exatamente 8 dígitos (ex: 01310100).");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número é obrigatório.")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Complemento)
            .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres.")
            .When(x => x.Complemento is not null);

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleFor(x => x.AreaM2)
            .GreaterThan(0).WithMessage("Área deve ser maior que zero.");

        RuleFor(x => x.Quartos)
            .GreaterThanOrEqualTo(0).WithMessage("Número de quartos não pode ser negativo.");
    }
}
