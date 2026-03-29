using Imoveis.Application.Features.Leads.RegistrarLead;
using Microsoft.AspNetCore.Mvc;

namespace Imoveis.API.Features.Leads;

[ApiController]
[Route("api/leads")]
[Produces("application/json")]
public class LeadController : ControllerBase
{
    private readonly RegistrarLeadHandler _handler;

    public LeadController(RegistrarLeadHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Registra interesse de um lead em um imóvel.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarLeadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarLeadCommand command,
        CancellationToken ct)
    {
        var result = await _handler.HandleAsync(command, ct);

        if (!result.Sucesso)
        {
            if (result.Erro!.Contains("não encontrado"))
                return NotFound(new { erro = result.Erro });

            return BadRequest(new { erro = result.Erro });
        }

        return Created($"/api/leads/{result.Dado!.LeadId}", result.Dado);
    }
}
