using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZelaBairro.Api.Data;
using ZelaBairro.Api.Domain;
using ZelaBairro.Api.Dtos;

namespace ZelaBairro.Api.Controllers;

[ApiController]
[Route("api/moradores")]
public class MoradoresController(AppDbContext db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MoradorDto>> Obter(int id)
    {
        var morador = await db.Moradores.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (morador is null) return NotFound(new { erro = "Morador não encontrado." });
        return Ok(MoradorDto.De(morador));
    }

    /// <summary>
    /// Identificação simples usada pelo app: se o e-mail já existe, atualiza nome/bairro;
    /// senão, cadastra. Devolve o morador para o app guardar localmente.
    /// </summary>
    [HttpPost("identificar")]
    public async Task<ActionResult<MoradorDto>> Identificar(MoradorIdentificacaoDto dto)
    {
        if (!await db.Bairros.AnyAsync(b => b.Id == dto.BairroId))
            throw new DomainException("Bairro não encontrado.");

        var email = Morador.NormalizarEmail(dto.Email);
        var morador = await db.Moradores.FirstOrDefaultAsync(m => m.Email == email);

        if (morador is null)
        {
            morador = new Morador(dto.Nome, email, dto.BairroId);
            db.Moradores.Add(morador);
        }
        else
        {
            morador.Atualizar(dto.Nome, dto.BairroId);
        }

        await db.SaveChangesAsync();
        return Ok(MoradorDto.De(morador));
    }
}
