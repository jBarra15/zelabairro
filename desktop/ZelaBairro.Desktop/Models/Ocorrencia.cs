namespace ZelaBairro.Desktop.Models;

/// <summary>
/// Ocorrência vista pelo terminal da subprefeitura.
/// As regras de transição ficam aqui também, para o operador só ver os botões permitidos
/// (a API continua sendo a autoridade final).
/// </summary>
public class Ocorrencia
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public StatusOcorrencia Status { get; set; }
    public Prioridade Prioridade { get; set; }
    public OrigemOcorrencia Origem { get; set; }
    public int Confirmacoes { get; set; }
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int BairroId { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public int? MoradorId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Endereco { get; set; }
    public bool TemFoto { get; set; }
    public bool Atrasada { get; set; }
    public DateTime CriadaEm { get; set; }
    public DateTime? ResolvidaEm { get; set; }

    public bool EstaEncerrada => Status is StatusOcorrencia.Resolvida or StatusOcorrencia.Cancelada;
    public bool PodeIniciarAtendimento => Status == StatusOcorrencia.Aberta;
    public bool PodeResolver => Status == StatusOcorrencia.EmAtendimento;
    public bool PodeCancelar => !EstaEncerrada;
    public bool PodePriorizar => !EstaEncerrada;

    /// <summary>Dias desde a abertura (a API grava em UTC).</summary>
    public int DiasEmAberto(DateTime agoraUtc) =>
        (int)Math.Floor(((ResolvidaEm ?? agoraUtc) - DateTime.SpecifyKind(CriadaEm, DateTimeKind.Utc)).TotalDays);
}
