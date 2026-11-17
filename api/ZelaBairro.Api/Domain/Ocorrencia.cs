namespace ZelaBairro.Api.Domain;

/// <summary>
/// Problema do espaço público registrado por um morador, sensor ou operador.
/// Ciclo de vida: Aberta -> EmAtendimento -> Resolvida (ou Cancelada a qualquer momento antes de resolver).
/// </summary>
public class Ocorrencia
{
    public int Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;

    /// <summary>Opcional. O Oracle trata texto vazio como NULL, então "sem descrição" é gravado como null.</summary>
    public string? Descricao { get; private set; }

    public int CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }

    public int BairroId { get; private set; }
    public Bairro? Bairro { get; private set; }

    public int? MoradorId { get; private set; }
    public Morador? Morador { get; private set; }

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? Endereco { get; private set; }
    public string? FotoArquivo { get; private set; }

    public StatusOcorrencia Status { get; private set; }
    public Prioridade Prioridade { get; private set; }
    public OrigemOcorrencia Origem { get; private set; }

    /// <summary>Quantos vizinhos confirmaram que o problema existe.</summary>
    public int Confirmacoes { get; private set; }

    public DateTime CriadaEm { get; private set; }
    public DateTime AtualizadaEm { get; private set; }
    public DateTime? ResolvidaEm { get; private set; }

    private Ocorrencia() { } // EF Core

    public static Ocorrencia Criar(
        string titulo, string? descricao, int categoriaId, int bairroId, int? moradorId,
        double latitude, double longitude, string? endereco, OrigemOcorrencia origem)
    {
        titulo = (titulo ?? "").Trim();
        descricao = (descricao ?? "").Trim();
        endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();

        if (titulo.Length is < 3 or > 100)
            throw new DomainException("Título deve ter entre 3 e 100 caracteres.");
        if (descricao.Length > 500)
            throw new DomainException("Descrição deve ter no máximo 500 caracteres.");
        if (endereco is { Length: > 200 })
            throw new DomainException("Endereço deve ter no máximo 200 caracteres.");
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new DomainException("Coordenadas inválidas.");
        if (origem == OrigemOcorrencia.App && moradorId is null)
            throw new DomainException("Ocorrências abertas pelo app precisam de um morador identificado.");

        var agora = DateTime.UtcNow;
        return new Ocorrencia
        {
            Titulo = titulo,
            Descricao = descricao.Length == 0 ? null : descricao,
            CategoriaId = categoriaId,
            BairroId = bairroId,
            MoradorId = moradorId,
            Latitude = latitude,
            Longitude = longitude,
            Endereco = endereco,
            Origem = origem,
            Status = StatusOcorrencia.Aberta,
            Prioridade = Prioridade.NaoDefinida,
            Confirmacoes = 0,
            CriadaEm = agora,
            AtualizadaEm = agora
        };
    }

    public bool EstaEncerrada => Status is StatusOcorrencia.Resolvida or StatusOcorrencia.Cancelada;

    public void Confirmar()
    {
        if (EstaEncerrada) throw new DomainException("Não é possível confirmar uma ocorrência encerrada.");
        Confirmacoes++;
        Tocar();
    }

    public void IniciarAtendimento()
    {
        if (Status != StatusOcorrencia.Aberta)
            throw new DomainException("Só ocorrências abertas podem entrar em atendimento.");
        Status = StatusOcorrencia.EmAtendimento;
        Tocar();
    }

    public void Resolver()
    {
        if (Status != StatusOcorrencia.EmAtendimento)
            throw new DomainException("A ocorrência precisa estar em atendimento para ser resolvida.");
        Status = StatusOcorrencia.Resolvida;
        ResolvidaEm = DateTime.UtcNow;
        Tocar();
    }

    public void Cancelar()
    {
        if (EstaEncerrada) throw new DomainException("A ocorrência já está encerrada.");
        Status = StatusOcorrencia.Cancelada;
        Tocar();
    }

    public void DefinirPrioridade(Prioridade prioridade)
    {
        if (EstaEncerrada) throw new DomainException("Não é possível priorizar uma ocorrência encerrada.");
        Prioridade = prioridade;
        Tocar();
    }

    public void AnexarFoto(string nomeArquivo)
    {
        FotoArquivo = nomeArquivo;
        Tocar();
    }

    /// <summary>Aberta ou em atendimento há mais tempo que o prazo da categoria.</summary>
    public bool EstaAtrasada(DateTime agora) =>
        Categoria is not null
        && !EstaEncerrada
        && (agora - CriadaEm).TotalDays > Categoria.PrazoAtendimentoDias;

    public bool FoiResolvidaNoPrazo() =>
        Categoria is not null
        && ResolvidaEm is not null
        && (ResolvidaEm.Value - CriadaEm).TotalDays <= Categoria.PrazoAtendimentoDias;

    private void Tocar() => AtualizadaEm = DateTime.UtcNow;
}