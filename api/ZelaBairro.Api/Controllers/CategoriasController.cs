using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZelaBairro.Api.Data;
using ZelaBairro.Api.Dtos;

namespace ZelaBairro.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> Listar() =>
        Ok(await db.Categorias.AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Descricao, c.PrazoAtendimentoDias))
            .ToListAsync());
}
