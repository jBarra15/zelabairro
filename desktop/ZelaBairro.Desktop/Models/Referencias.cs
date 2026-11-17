namespace ZelaBairro.Desktop.Models;

public class Bairro
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;

    public override string ToString() => Nome;
}

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int PrazoAtendimentoDias { get; set; }

    public override string ToString() => $"{Nome} (prazo {PrazoAtendimentoDias} dias)";
}

public class Resumo
{
    public int Total { get; set; }
    public int Abertas { get; set; }
    public int EmAtendimento { get; set; }
    public int Resolvidas { get; set; }
    public int Canceladas { get; set; }
    public int Atrasadas { get; set; }
    public double PercentualResolvidasNoPrazo { get; set; }
}
