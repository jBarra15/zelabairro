using System.Text.Json;
using System.Text.Json.Serialization;
using ZelaBairro.Desktop.Models;

namespace ZelaBairro.Desktop.Services;

/// <summary>
/// Persistência local em JSON (pasta %AppData%\ZelaBairro):
///  - historico.json: tudo que o operador fez no terminal (auditoria do atendimento);
///  - cache-ocorrencias.json: última lista recebida, para consulta se a API cair.
/// </summary>
public class RegistroLocal
{
    private readonly string _pasta;
    private string ArquivoHistorico => Path.Combine(_pasta, "historico.json");
    private string ArquivoCache => Path.Combine(_pasta, "cache-ocorrencias.json");

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public RegistroLocal()
    {
        _pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZelaBairro");
        Directory.CreateDirectory(_pasta);
    }

    public string Pasta => _pasta;

    // ---------- Histórico ----------
    public void RegistrarAcao(string operador, int ocorrenciaId, string acao, string detalhe)
    {
        var historico = LerHistorico();
        historico.Add(new RegistroAcao
        {
            DataHora = DateTime.Now,
            Operador = operador,
            OcorrenciaId = ocorrenciaId,
            Acao = acao,
            Detalhe = detalhe
        });
        Salvar(ArquivoHistorico, historico);
    }

    public List<RegistroAcao> LerHistorico() => Ler<List<RegistroAcao>>(ArquivoHistorico) ?? new List<RegistroAcao>();

    // ---------- Cache ----------
    public void SalvarCache(List<Ocorrencia> ocorrencias) => Salvar(ArquivoCache, ocorrencias);

    public List<Ocorrencia>? LerCache() => Ler<List<Ocorrencia>>(ArquivoCache);

    public DateTime? DataDoCache() => File.Exists(ArquivoCache) ? File.GetLastWriteTime(ArquivoCache) : null;

    // ---------- Infra ----------
    private static void Salvar<T>(string caminho, T dados)
    {
        // Grava num temporário e troca: um travamento no meio não corrompe o arquivo.
        var temporario = caminho + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(dados, Json));
        File.Move(temporario, caminho, overwrite: true);
    }

    private static T? Ler<T>(string caminho) where T : class
    {
        if (!File.Exists(caminho)) return null;
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(caminho), Json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
