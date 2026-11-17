namespace ZelaBairro.Api.Domain;

public class Morador
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public int BairroId { get; private set; }
    public Bairro? Bairro { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Morador() { } // EF Core

    public Morador(string nome, string email, int bairroId)
    {
        DefinirNome(nome);
        Email = NormalizarEmail(email);
        BairroId = bairroId;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, int bairroId)
    {
        DefinirNome(nome);
        BairroId = bairroId;
    }

    public static string NormalizarEmail(string email)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        if (e.Length is < 5 or > 120 || !e.Contains('@') || e.StartsWith('@') || e.EndsWith('@'))
            throw new DomainException("E-mail inválido.");
        return e;
    }

    private void DefinirNome(string nome)
    {
        var n = (nome ?? "").Trim();
        if (n.Length is < 2 or > 80) throw new DomainException("Nome deve ter entre 2 e 80 caracteres.");
        Nome = n;
    }
}
