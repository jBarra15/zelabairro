namespace ZelaBairro.Desktop.Models;

/// <summary>Linha do histórico local de atendimento (quem fez o quê e quando).</summary>
public class RegistroAcao
{
    public DateTime DataHora { get; set; }
    public string Operador { get; set; } = string.Empty;
    public int OcorrenciaId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Detalhe { get; set; } = string.Empty;
}
