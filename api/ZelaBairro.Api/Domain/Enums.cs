namespace ZelaBairro.Api.Domain;

public enum StatusOcorrencia
{
    Aberta,
    EmAtendimento,
    Resolvida,
    Cancelada
}

public enum Prioridade
{
    NaoDefinida,
    Baixa,
    Media,
    Alta
}

/// <summary>Quem abriu a ocorrência: morador pelo app, sensor ESP32 ou operador no terminal WinForms.</summary>
public enum OrigemOcorrencia
{
    App,
    Sensor,
    Terminal
}
