using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZelaBairro.Api.Data;
using ZelaBairro.Api.Domain;
using ZelaBairro.Api.Dtos;

namespace ZelaBairro.Api.Controllers;

[ApiController]
[Route("api/ocorrencias")]
public class OcorrenciasController(AppDbContext db, IWebHostEnvironment env, IConfiguration config) : ControllerBase
{
    private const long TamanhoMaximoFoto = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private string PastaUploads =>
        Path.GetFullPath(Path.Combine(env.ContentRootPath, config["Uploads:Pasta"] ?? "uploads"));

    private IQueryable<Ocorrencia> ComDetalhes() =>
        db.Ocorrencias.Include(o => o.Categoria).Include(o => o.Bairro);

    private Task<Ocorrencia?> Buscar(int id) =>
        ComDetalhes().FirstOrDefaultAsync(o => o.Id == id);

    private static NotFoundObjectResult NaoEncontrada() =>
        new(new { erro = "Ocorrência não encontrada." });

    // GET api/ocorrencias?bairroId=1&categoriaId=2&moradorId=3&status=Aberta
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OcorrenciaDto>>> Listar(
        [FromQuery] int? bairroId,
        [FromQuery] int? categoriaId,
        [FromQuery] int? moradorId,
        [FromQuery] StatusOcorrencia? status)
    {
        var query = ComDetalhes().AsNoTracking();
        if (bairroId is not null) query = query.Where(o => o.BairroId == bairroId);
        if (categoriaId is not null) query = query.Where(o => o.CategoriaId == categoriaId);
        if (moradorId is not null) query = query.Where(o => o.MoradorId == moradorId);
        if (status is not null) query = query.Where(o => o.Status == status);

        var lista = await query.OrderByDescending(o => o.CriadaEm).ToListAsync();
        var agora = DateTime.UtcNow;
        return Ok(lista.Select(o => OcorrenciaDto.De(o, agora)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OcorrenciaDto>> Obter(int id)
    {
        var ocorrencia = await ComDetalhes().AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (ocorrencia is null) return NaoEncontrada();
        return Ok(OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    [HttpPost]
    public async Task<ActionResult<OcorrenciaDto>> Criar(OcorrenciaCreateDto dto)
    {
        if (!await db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            throw new DomainException("Categoria não encontrada.");
        if (!await db.Bairros.AnyAsync(b => b.Id == dto.BairroId))
            throw new DomainException("Bairro não encontrado.");
        if (dto.MoradorId is not null && !await db.Moradores.AnyAsync(m => m.Id == dto.MoradorId))
            throw new DomainException("Morador não encontrado.");

        var ocorrencia = Ocorrencia.Criar(
            dto.Titulo, dto.Descricao, dto.CategoriaId, dto.BairroId, dto.MoradorId,
            dto.Latitude, dto.Longitude, dto.Endereco, dto.Origem);

        db.Ocorrencias.Add(ocorrencia);
        await db.SaveChangesAsync();

        await db.Entry(ocorrencia).Reference(o => o.Categoria).LoadAsync();
        await db.Entry(ocorrencia).Reference(o => o.Bairro).LoadAsync();

        return CreatedAtAction(nameof(Obter), new { id = ocorrencia.Id }, OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OcorrenciaDto>> AlterarStatus(int id, StatusUpdateDto dto)
    {
        var ocorrencia = await Buscar(id);
        if (ocorrencia is null) return NaoEncontrada();

        switch (dto.Status)
        {
            case StatusOcorrencia.EmAtendimento: ocorrencia.IniciarAtendimento(); break;
            case StatusOcorrencia.Resolvida: ocorrencia.Resolver(); break;
            case StatusOcorrencia.Cancelada: ocorrencia.Cancelar(); break;
            default: throw new DomainException("Não é possível voltar uma ocorrência para 'Aberta'.");
        }

        await db.SaveChangesAsync();
        return Ok(OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    [HttpPatch("{id:int}/prioridade")]
    public async Task<ActionResult<OcorrenciaDto>> DefinirPrioridade(int id, PrioridadeUpdateDto dto)
    {
        var ocorrencia = await Buscar(id);
        if (ocorrencia is null) return NaoEncontrada();

        ocorrencia.DefinirPrioridade(dto.Prioridade);
        await db.SaveChangesAsync();
        return Ok(OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    /// <summary>Um vizinho confirma que o problema existe (ajuda a priorizar).</summary>
    [HttpPost("{id:int}/confirmar")]
    public async Task<ActionResult<OcorrenciaDto>> Confirmar(int id)
    {
        var ocorrencia = await Buscar(id);
        if (ocorrencia is null) return NaoEncontrada();

        ocorrencia.Confirmar();
        await db.SaveChangesAsync();
        return Ok(OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    // ---------- Foto (com proteções contra upload malicioso e LFI/path traversal) ----------

    [HttpPost("{id:int}/foto")]
    [RequestSizeLimit(TamanhoMaximoFoto + 64 * 1024)]
    public async Task<ActionResult<OcorrenciaDto>> EnviarFoto(int id, IFormFile foto)
    {
        var ocorrencia = await Buscar(id);
        if (ocorrencia is null) return NaoEncontrada();

        if (foto is null || foto.Length == 0)
            throw new DomainException("Envie uma imagem no campo 'foto'.");
        if (foto.Length > TamanhoMaximoFoto)
            throw new DomainException("A imagem deve ter no máximo 5 MB.");

        var extensao = Path.GetExtension(foto.FileName);
        if (!TiposPermitidos.ContainsKey(extensao))
            throw new DomainException("Formato não permitido. Use JPG ou PNG.");
        if (!await AssinaturaDeImagemValida(foto))
            throw new DomainException("O conteúdo do arquivo não é uma imagem JPG/PNG válida.");

        // O nome enviado pelo cliente é descartado: geramos um nome próprio.
        var nomeArquivo = $"{Guid.NewGuid():N}{extensao.ToLowerInvariant()}";
        Directory.CreateDirectory(PastaUploads);
        await using (var destino = System.IO.File.Create(Path.Combine(PastaUploads, nomeArquivo)))
        {
            await foto.CopyToAsync(destino);
        }

        RemoverArquivo(ocorrencia.FotoArquivo);
        ocorrencia.AnexarFoto(nomeArquivo);
        await db.SaveChangesAsync();

        return Ok(OcorrenciaDto.De(ocorrencia, DateTime.UtcNow));
    }

    [HttpGet("{id:int}/foto")]
    public async Task<IActionResult> ObterFoto(int id)
    {
        var ocorrencia = await db.Ocorrencias.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (ocorrencia?.FotoArquivo is null) return NotFound(new { erro = "Foto não encontrada." });

        var caminho = CaminhoSeguro(ocorrencia.FotoArquivo);
        if (caminho is null || !System.IO.File.Exists(caminho))
            return NotFound(new { erro = "Foto não encontrada." });

        var tipo = TiposPermitidos.GetValueOrDefault(Path.GetExtension(caminho), "application/octet-stream");
        return PhysicalFile(caminho, tipo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var ocorrencia = await db.Ocorrencias.FirstOrDefaultAsync(o => o.Id == id);
        if (ocorrencia is null) return NaoEncontrada();

        db.Ocorrencias.Remove(ocorrencia);
        await db.SaveChangesAsync();
        RemoverArquivo(ocorrencia.FotoArquivo);
        return NoContent();
    }

    // ---------- Indicador de impacto ----------

    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoDto>> Resumo()
    {
        var lista = await ComDetalhes().AsNoTracking().ToListAsync();
        var agora = DateTime.UtcNow;
        var resolvidas = lista.Where(o => o.Status == StatusOcorrencia.Resolvida).ToList();
        var noPrazo = resolvidas.Count(o => o.FoiResolvidaNoPrazo());

        return Ok(new ResumoDto(
            Total: lista.Count,
            Abertas: lista.Count(o => o.Status == StatusOcorrencia.Aberta),
            EmAtendimento: lista.Count(o => o.Status == StatusOcorrencia.EmAtendimento),
            Resolvidas: resolvidas.Count,
            Canceladas: lista.Count(o => o.Status == StatusOcorrencia.Cancelada),
            Atrasadas: lista.Count(o => o.EstaAtrasada(agora)),
            PercentualResolvidasNoPrazo: resolvidas.Count == 0 ? 0 : Math.Round(100.0 * noPrazo / resolvidas.Count, 1)));
    }

    // ---------- Auxiliares ----------

    /// <summary>Garante que o caminho final fica DENTRO da pasta de uploads (bloqueia ../../).</summary>
    private string? CaminhoSeguro(string nomeArquivo)
    {
        var caminho = Path.GetFullPath(Path.Combine(PastaUploads, Path.GetFileName(nomeArquivo)));
        return caminho.StartsWith(PastaUploads + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            ? caminho
            : null;
    }

    private void RemoverArquivo(string? nomeArquivo)
    {
        if (nomeArquivo is null) return;
        var caminho = CaminhoSeguro(nomeArquivo);
        if (caminho is not null && System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
    }

    /// <summary>Confere os "magic bytes": não basta a extensão ser .jpg, o conteúdo precisa ser imagem.</summary>
    private static async Task<bool> AssinaturaDeImagemValida(IFormFile arquivo)
    {
        var cabecalho = new byte[4];
        await using var stream = arquivo.OpenReadStream();
        var lidos = await stream.ReadAsync(cabecalho);
        if (lidos < 4) return false;

        var jpeg = cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF;
        var png = cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E && cabecalho[3] == 0x47;
        return jpeg || png;
    }
}
