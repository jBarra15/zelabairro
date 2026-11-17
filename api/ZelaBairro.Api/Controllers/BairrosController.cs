using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZelaBairro.Api.Data;
using ZelaBairro.Api.Dtos;

namespace ZelaBairro.Api.Controllers;

[ApiController]
[Route("api/bairros")]
public class BairrosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BairroDto>>> Listar() =>
        Ok(await db.Bairros.AsNoTracking()
            .OrderBy(b => b.Nome)
            .Select(b => new BairroDto(b.Id, b.Nome, b.Cidade))
            .ToListAsync());
}
