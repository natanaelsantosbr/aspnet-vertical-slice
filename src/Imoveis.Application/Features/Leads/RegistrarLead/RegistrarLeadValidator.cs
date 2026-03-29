using FluentValidation;

namespace Imoveis.Application.Features.Leads.RegistrarLead;

public class RegistrarLeadValidator : AbstractValidator<RegistrarLeadCommand>
{
    public RegistrarLeadValidator()
    {
        RuleFor(x => x.ImovelId)
            .NotEmpty().WithMessage("ImovelId é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Mensagem)
            .MaximumLength(1000).WithMessage("Mensagem deve ter no máximo 1000 caracteres.")
            .When(x => x.Mensagem != null);
    }
}
