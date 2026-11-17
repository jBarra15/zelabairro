using System.ComponentModel.DataAnnotations;
using ZelaBairro.Api.Domain;

namespace ZelaBairro.Api.Dtos;

// ---------- Referência ----------
public record BairroDto(int Id, string Nome, string Cidade);

public record CategoriaDto(int Id, string Nome, string Descricao, int PrazoAtendimentoDias);

// ---------- Morador ----------
public record MoradorIdentificacaoDto(
    [Required, StringLength(80, MinimumLength = 2)] string Nome,
    [Required, EmailAddress, StringLength(120)] string Email,
    [Range(1, int.MaxValue)] int BairroId);

public record MoradorDto(int Id, string Nome, string Email, int BairroId, DateTime CriadoEm)
{
    public static MoradorDto De(Morador m) => new(m.Id, m.Nome, m.Email, m.BairroId, m.CriadoEm);
}

// ---------- Ocorrência ----------
public record OcorrenciaCreateDto(
    [Required, StringLength(100, MinimumLength = 3)] string Titulo,
    [StringLength(500)] string? Descricao,
    [Range(1, int.MaxValue)] int CategoriaId,
    [Range(1, int.MaxValue)] int BairroId,
    int? MoradorId,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    [StringLength(200)] string? Endereco,
    OrigemOcorrencia Origem = OrigemOcorrencia.App);

public record StatusUpdateDto([Required] StatusOcorrencia Status);

public record PrioridadeUpdateDto([Required] Prioridade Prioridade);

public record OcorrenciaDto(
    int Id,
    string Titulo,
    string? Descricao,
    StatusOcorrencia Status,
    Prioridade Prioridade,
    OrigemOcorrencia Origem,
    int Confirmacoes,
    int CategoriaId,
    string Categoria,
    int BairroId,
    string Bairro,
    int? MoradorId,
    double Latitude,
    double Longitude,
    string? Endereco,
    bool TemFoto,
    bool Atrasada,
    DateTime CriadaEm,
    DateTime? ResolvidaEm)
{
    public static OcorrenciaDto De(Ocorrencia o, DateTime agora) => new(
        o.Id, o.Titulo, o.Descricao, o.Status, o.Prioridade, o.Origem, o.Confirmacoes,
        o.CategoriaId, o.Categoria?.Nome ?? "", o.BairroId, o.Bairro?.Nome ?? "", o.MoradorId,
        o.Latitude, o.Longitude, o.Endereco, o.FotoArquivo is not null, o.EstaAtrasada(agora),
        o.CriadaEm, o.ResolvidaEm);
}

/// <summary>Indicador de impacto da CitizenTech.</summary>
public record ResumoDto(
    int Total,
    int Abertas,
    int EmAtendimento,
    int Resolvidas,
    int Canceladas,
    int Atrasadas,
    double PercentualResolvidasNoPrazo);