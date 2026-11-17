using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZelaBairro.Desktop.Models;

namespace ZelaBairro.Desktop.Services;

/// <summary>Erro com mensagem pronta para mostrar ao operador.</summary>
public class ApiException(string mensagem) : Exception(mensagem);

/// <summary>Toda a comunicação com a API ZelaBairro passa por aqui.</summary>
public class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public string Endereco => _http.BaseAddress?.ToString() ?? "";

    // ---------- Consultas ----------
    public Task<List<Bairro>> ListarBairrosAsync() => GetAsync<List<Bairro>>("api/bairros");
    public Task<List<Categoria>> ListarCategoriasAsync() => GetAsync<List<Categoria>>("api/categorias");
    public Task<Resumo> ObterResumoAsync() => GetAsync<Resumo>("api/ocorrencias/resumo");

    public Task<List<Ocorrencia>> ListarOcorrenciasAsync(int? bairroId, StatusOcorrencia? status)
    {
        var filtros = new List<string>();
        if (bairroId is not null) filtros.Add($"bairroId={bairroId}");
        if (status is not null) filtros.Add($"status={status}");
        var query = filtros.Count > 0 ? "?" + string.Join("&", filtros) : "";
        return GetAsync<List<Ocorrencia>>("api/ocorrencias" + query);
    }

    // ---------- Ações do operador ----------
    public Task<Ocorrencia> AlterarStatusAsync(int id, StatusOcorrencia status) =>
        SendAsync<Ocorrencia>(HttpMethod.Patch, $"api/ocorrencias/{id}/status", new { status });

    public Task<Ocorrencia> DefinirPrioridadeAsync(int id, Prioridade prioridade) =>
        SendAsync<Ocorrencia>(HttpMethod.Patch, $"api/ocorrencias/{id}/prioridade", new { prioridade });

    public Task<Ocorrencia> CriarOcorrenciaAsync(object dados) =>
        SendAsync<Ocorrencia>(HttpMethod.Post, "api/ocorrencias", dados);

    public async Task<Image?> ObterFotoAsync(int id)
    {
        try
        {
            var resposta = await _http.GetAsync($"api/ocorrencias/{id}/foto");
            if (!resposta.IsSuccessStatusCode) return null;
            var bytes = await resposta.Content.ReadAsByteArrayAsync();
            using var ms = new MemoryStream(bytes);
            // Copia para um Bitmap independente do stream (o stream é descartado aqui).
            using var original = Image.FromStream(ms);
            return new Bitmap(original);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or ArgumentException)
        {
            return null;
        }
    }

    // ---------- Infra ----------
    private async Task<T> GetAsync<T>(string url)
    {
        HttpResponseMessage resposta;
        try
        {
            resposta = await _http.GetAsync(url);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException($"Sem conexão com a API em {Endereco}. Verifique se ela está rodando.");
        }
        return await LerAsync<T>(resposta);
    }

    private async Task<T> SendAsync<T>(HttpMethod metodo, string url, object corpo)
    {
        HttpResponseMessage resposta;
        try
        {
            using var requisicao = new HttpRequestMessage(metodo, url)
            {
                Content = JsonContent.Create(corpo, options: Json)
            };
            resposta = await _http.SendAsync(requisicao);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException($"Sem conexão com a API em {Endereco}. Verifique se ela está rodando.");
        }
        return await LerAsync<T>(resposta);
    }

    private static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
    {
        var texto = await resposta.Content.ReadAsStringAsync();

        if (!resposta.IsSuccessStatusCode)
            throw new ApiException(ExtrairMensagem(texto, (int)resposta.StatusCode));

        try
        {
            return JsonSerializer.Deserialize<T>(texto, Json)
                   ?? throw new ApiException("A API respondeu em um formato inesperado.");
        }
        catch (JsonException)
        {
            throw new ApiException("A API respondeu em um formato inesperado.");
        }
    }

    /// <summary>A API devolve { erro } nas regras de negócio e { errors: {...} } na validação automática.</summary>
    private static string ExtrairMensagem(string texto, int status)
    {
        try
        {
            using var doc = JsonDocument.Parse(texto);
            var raiz = doc.RootElement;
            if (raiz.TryGetProperty("erro", out var erro) && erro.ValueKind == JsonValueKind.String)
                return erro.GetString()!;

            if (raiz.TryGetProperty("errors", out var erros) && erros.ValueKind == JsonValueKind.Object)
                foreach (var campo in erros.EnumerateObject())
                    if (campo.Value.ValueKind == JsonValueKind.Array)
                        foreach (var msg in campo.Value.EnumerateArray())
                            if (msg.ValueKind == JsonValueKind.String) return msg.GetString()!;
        }
        catch (JsonException)
        {
            // corpo não é JSON
        }

        return status switch
        {
            404 => "Registro não encontrado.",
            >= 500 => "A API está com problemas. Tente novamente.",
            _ => "Não foi possível concluir a operação."
        };
    }
}
