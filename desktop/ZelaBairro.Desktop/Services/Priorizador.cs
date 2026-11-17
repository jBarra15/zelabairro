using ZelaBairro.Desktop.Models;

namespace ZelaBairro.Desktop.Services;

/// <summary>Resultado da triagem: prioridade sugerida e o porquê, para o operador decidir.</summary>
public record SugestaoPrioridade(Prioridade Prioridade, int Pontos, IReadOnlyList<string> Motivos);

/// <summary>
/// Algoritmo de triagem: soma pontos por risco à pessoa, tempo em aberto,
/// confirmações de vizinhos e origem, e converte a pontuação em prioridade.
/// (Na entrega de IA, o modelo de ML treinado substitui/valida esta regra.)
/// </summary>
public static class Priorizador
{
    // Pontos de risco por categoria (ids do seed da API).
    private static readonly Dictionary<int, (int Pontos, string Motivo)> RiscoPorCategoria = new()
    {
        [1] = (2, "lixo acumulado atrai vetores de doença"),
        [2] = (3, "rua escura aumenta risco à noite"),
        [3] = (3, "buraco pode causar acidente"),
        [4] = (2, "calçada danificada causa quedas de idosos"),
        [5] = (4, "árvore pode cair sobre pessoas ou fiação")
    };

    public static SugestaoPrioridade Sugerir(Ocorrencia ocorrencia, int prazoCategoriaDias, DateTime agoraUtc)
    {
        var pontos = 0;
        var motivos = new List<string>();

        if (RiscoPorCategoria.TryGetValue(ocorrencia.CategoriaId, out var risco))
        {
            pontos += risco.Pontos;
            motivos.Add($"+{risco.Pontos} {risco.Motivo}");
        }

        var dias = ocorrencia.DiasEmAberto(agoraUtc);
        if (prazoCategoriaDias > 0)
        {
            var usoDoPrazo = (double)dias / prazoCategoriaDias;
            if (usoDoPrazo >= 1)
            {
                pontos += 3;
                motivos.Add($"+3 prazo de {prazoCategoriaDias} dias estourado ({dias} dias em aberto)");
            }
            else if (usoDoPrazo >= 0.5)
            {
                pontos += 1;
                motivos.Add($"+1 já usou metade do prazo ({dias} de {prazoCategoriaDias} dias)");
            }
        }

        var bonusConfirmacoes = Math.Min(ocorrencia.Confirmacoes, 3);
        if (bonusConfirmacoes > 0)
        {
            pontos += bonusConfirmacoes;
            motivos.Add($"+{bonusConfirmacoes} {ocorrencia.Confirmacoes} vizinho(s) confirmaram");
        }

        if (ocorrencia.Origem == OrigemOcorrencia.Sensor)
        {
            pontos += 1;
            motivos.Add("+1 medido por sensor (problema comprovado)");
        }

        var prioridade = pontos switch
        {
            >= 7 => Prioridade.Alta,
            >= 4 => Prioridade.Media,
            _ => Prioridade.Baixa
        };

        return new SugestaoPrioridade(prioridade, pontos, motivos);
    }
}
