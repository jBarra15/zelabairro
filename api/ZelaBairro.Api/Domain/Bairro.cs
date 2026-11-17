namespace ZelaBairro.Api.Domain;

public class Bairro
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public ICollection<Ocorrencia> Ocorrencias { get; private set; } = new List<Ocorrencia>();

    private Bairro() { } // EF Core

    public Bairro(string nome, string cidade)
    {
        nome = (nome ?? "").Trim();
        cidade = (cidade ?? "").Trim();
        if (nome.Length is < 2 or > 80) throw new DomainException("Nome do bairro inválido.");
        if (cidade.Length is < 2 or > 80) throw new DomainException("Cidade inválida.");
        Nome = nome;
        Cidade = cidade;
    }
}
