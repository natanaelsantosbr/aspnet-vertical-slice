using FluentValidation;
using Imoveis.Application.Common;
using Imoveis.Domain.Entities;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Leads.RegistrarLead;

public class RegistrarLeadHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<RegistrarLeadCommand> _validator;
    private readonly ILogger<RegistrarLeadHandler> _logger;

    public RegistrarLeadHandler(
        AppDbContext db,
        IValidator<RegistrarLeadCommand> validator,
        ILogger<RegistrarLeadHandler> logger)
    {
        _db = db;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<RegistrarLeadResponse>> HandleAsync(
        RegistrarLeadCommand command,
        CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(command, ct);
        if (!validacao.IsValid)
        {
            var erros = string.Join("; ", validacao.Errors.Select(e => e.ErrorMessage));
            return Result<RegistrarLeadResponse>.Falha(erros);
        }

        var imovelExiste = await _db.Imoveis
            .AnyAsync(i => i.Id == command.ImovelId && i.Ativo, ct);

        if (!imovelExiste)
            return Result<RegistrarLeadResponse>.Falha("Imóvel não encontrado ou inativo.");

        var lead = Lead.Criar(
            command.ImovelId,
            command.Nome,
            command.Email,
            command.Telefone,
            command.Mensagem);

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Lead registrado: {LeadId} | Imóvel: {ImovelId} | Contato: {Email}",
            lead.Id, command.ImovelId, command.Email);

        return Result<RegistrarLeadResponse>.Ok(new RegistrarLeadResponse(lead.Id));
    }
}
