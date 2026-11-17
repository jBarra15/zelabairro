namespace ZelaBairro.Api.Domain;

public class Categoria
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    /// <summary>Prazo (SLA) em dias para resolver ocorrências desta categoria.</summary>
    public int PrazoAtendimentoDias { get; private set; }

    private Categoria() { } // EF Core

    public Categoria(string nome, string descricao, int prazoAtendimentoDias)
    {
        nome = (nome ?? "").Trim();
        if (nome.Length is < 2 or > 60) throw new DomainException("Nome da categoria inválido.");
        if (prazoAtendimentoDias is < 1 or > 90) throw new DomainException("Prazo deve ficar entre 1 e 90 dias.");
        Nome = nome;
        Descricao = (descricao ?? "").Trim();
        PrazoAtendimentoDias = prazoAtendimentoDias;
    }
}
