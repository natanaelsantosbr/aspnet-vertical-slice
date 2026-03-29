namespace Imoveis.Application.Features.Leads.RegistrarLead;

public record RegistrarLeadCommand(
    Guid ImovelId,
    string Nome,
    string Email,
    string Telefone,
    string? Mensagem
);

public record RegistrarLeadResponse(Guid LeadId);
