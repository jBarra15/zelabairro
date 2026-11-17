using System.Text.Json;

namespace ZelaBairro.Desktop.Services;

/// <summary>Lê o zelabairro.config.json que fica ao lado do .exe.</summary>
public class Configuracao
{
    public string ApiUrl { get; set; } = "http://localhost:5080";
    public string Operador { get; set; } = "Operador";

    public static Configuracao Carregar()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "zelabairro.config.json");
        try
        {
            if (File.Exists(caminho))
            {
                var config = JsonSerializer.Deserialize<Configuracao>(File.ReadAllText(caminho));
                if (config is not null && Uri.IsWellFormedUriString(config.ApiUrl, UriKind.Absolute))
                    return config;
            }
        }
        catch (JsonException)
        {
            // arquivo inválido: usa os valores padrão
        }
        return new Configuracao();
    }
}
