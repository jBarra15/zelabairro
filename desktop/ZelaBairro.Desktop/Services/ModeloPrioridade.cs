using System.Text.Json;
using System.Text.Json.Serialization;
using ZelaBairro.Desktop.Models;

namespace ZelaBairro.Desktop.Services;

/// <summary>Resultado do modelo: classe prevista, confiança e os fatores que mais pesaram.</summary>
public record PrevisaoModelo(Prioridade Prioridade, double Confianca, IReadOnlyList<string> Fatores);

/// <summary>
/// Modelo de Machine Learning treinado no notebook ml/ZelaBairro_Prioridade.ipynb
/// (regressão logística multinomial + padronização), exportado em modelo_prioridade.json.
/// Aqui só aplicamos a conta: z = (x - média) / desvio; logit = intercepto + pesos · z; softmax.
/// </summary>
public class ModeloPrioridade
{
    private sealed class Arquivo
    {
        [JsonPropertyName("versao")] public string Versao { get; set; } = "";
        [JsonPropertyName("features")] public List<string> Features { get; set; } = new();
        [JsonPropertyName("classes")] public List<string> Classes { get; set; } = new();
        [JsonPropertyName("media")] public List<double> Media { get; set; } = new();
        [JsonPropertyName("desvio")] public List<double> Desvio { get; set; } = new();
        [JsonPropertyName("pesos")] public List<List<double>> Pesos { get; set; } = new();
        [JsonPropertyName("interceptos")] public List<double> Interceptos { get; set; } = new();
        [JsonPropertyName("metricas_teste")] public Dictionary<string, double> MetricasTeste { get; set; } = new();
    }

    private static readonly Dictionary<string, string> NomesAmigaveis = new()
    {
        ["cat_1"] = "categoria lixo",
        ["cat_2"] = "categoria iluminação",
        ["cat_3"] = "categoria buraco",
        ["cat_4"] = "categoria calçada",
        ["cat_5"] = "categoria árvore",
        ["prazo_dias"] = "prazo da categoria",
        ["dias_em_aberto"] = "dias em aberto",
        ["uso_prazo"] = "prazo consumido",
        ["confirmacoes"] = "confirmações de vizinhos",
        ["tem_foto"] = "tem foto",
        ["origem_sensor"] = "medido por sensor",
        ["origem_terminal"] = "registrado no balcão"
    };

    private readonly Arquivo _m;
    private readonly Prioridade[] _classes;

    private ModeloPrioridade(Arquivo m, Prioridade[] classes)
    {
        _m = m;
        _classes = classes;
    }

    public string Versao => _m.Versao;

    public double? F1Teste => _m.MetricasTeste.TryGetValue("F1-macro", out var f1) ? f1 : null;

    /// <summary>Carrega o modelo. Devolve null se o arquivo não existir ou estiver inconsistente.</summary>
    public static ModeloPrioridade? Carregar(string caminho)
    {
        try
        {
            if (!File.Exists(caminho)) return null;
            var m = JsonSerializer.Deserialize<Arquivo>(File.ReadAllText(caminho));
            if (m is null) return null;

            var n = m.Features.Count;
            var k = m.Classes.Count;
            var consistente = n > 0 && k > 1
                && m.Media.Count == n && m.Desvio.Count == n
                && m.Interceptos.Count == k && m.Pesos.Count == k
                && m.Pesos.All(linha => linha.Count == n)
                && m.Desvio.All(d => d > 0);
            if (!consistente) return null;

            var classes = new Prioridade[k];
            for (var i = 0; i < k; i++)
                if (!Enum.TryParse(m.Classes[i], out classes[i])) return null;

            return new ModeloPrioridade(m, classes);
        }
        catch (Exception e) when (e is JsonException or IOException)
        {
            return null;
        }
    }

    public PrevisaoModelo Prever(Ocorrencia o, int prazoCategoriaDias, DateTime agoraUtc)
    {
        var x = MontarFeatures(o, prazoCategoriaDias, agoraUtc);
        var n = _m.Features.Count;

        // Padronização (mesma média/desvio do treino)
        var z = new double[n];
        for (var j = 0; j < n; j++) z[j] = (x[j] - _m.Media[j]) / _m.Desvio[j];

        // Logits e softmax
        var logits = new double[_classes.Length];
        for (var k = 0; k < logits.Length; k++)
        {
            logits[k] = _m.Interceptos[k];
            for (var j = 0; j < n; j++) logits[k] += _m.Pesos[k][j] * z[j];
        }
        var maior = logits.Max();
        var exp = logits.Select(l => Math.Exp(l - maior)).ToArray();
        var soma = exp.Sum();
        var probs = exp.Select(e => e / soma).ToArray();

        var escolhida = Array.IndexOf(probs, probs.Max());

        // Fatores que mais empurraram para a classe escolhida (só os presentes nesta ocorrência)
        var fatores = Enumerable.Range(0, n)
            .Where(j => x[j] != 0)
            .Select(j => (Nome: _m.Features[j], Contribuicao: _m.Pesos[escolhida][j] * z[j]))
            .Where(f => f.Contribuicao > 0.05)
            .OrderByDescending(f => f.Contribuicao)
            .Take(3)
            .Select(f => NomesAmigaveis.GetValueOrDefault(f.Nome, f.Nome))
            .ToList();

        return new PrevisaoModelo(_classes[escolhida], probs[escolhida], fatores);
    }

    /// <summary>Mesmas 12 variáveis, na mesma ordem e definição do notebook (função montar_features).</summary>
    private double[] MontarFeatures(Ocorrencia o, int prazoDias, DateTime agoraUtc)
    {
        var dias = Math.Max(0, o.DiasEmAberto(agoraUtc));
        var prazo = prazoDias > 0 ? prazoDias : 1;

        var valores = new Dictionary<string, double>
        {
            ["cat_1"] = o.CategoriaId == 1 ? 1 : 0,
            ["cat_2"] = o.CategoriaId == 2 ? 1 : 0,
            ["cat_3"] = o.CategoriaId == 3 ? 1 : 0,
            ["cat_4"] = o.CategoriaId == 4 ? 1 : 0,
            ["cat_5"] = o.CategoriaId == 5 ? 1 : 0,
            ["prazo_dias"] = prazo,
            ["dias_em_aberto"] = dias,
            ["uso_prazo"] = (double)dias / prazo,
            ["confirmacoes"] = o.Confirmacoes,
            ["tem_foto"] = o.TemFoto ? 1 : 0,
            ["origem_sensor"] = o.Origem == OrigemOcorrencia.Sensor ? 1 : 0,
            ["origem_terminal"] = o.Origem == OrigemOcorrencia.Terminal ? 1 : 0
        };

        return _m.Features.Select(f => valores.GetValueOrDefault(f, 0)).ToArray();
    }
}
