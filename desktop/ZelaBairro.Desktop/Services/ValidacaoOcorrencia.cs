namespace ZelaBairro.Desktop.Services;

/// <summary>Validação do cadastro feito pelo operador (mesmas regras da API).</summary>
public static class ValidacaoOcorrencia
{
    public static List<string> Validar(string titulo, string descricao, string endereco, object? categoria, object? bairro)
    {
        var erros = new List<string>();
        titulo = titulo.Trim();

        if (titulo.Length is < 3 or > 100) erros.Add("Título deve ter entre 3 e 100 caracteres.");
        if (descricao.Trim().Length > 500) erros.Add("Descrição deve ter no máximo 500 caracteres.");
        if (endereco.Trim().Length is < 5 or > 200) erros.Add("Informe o endereço ou ponto de referência (5 a 200 caracteres).");
        if (categoria is null) erros.Add("Escolha a categoria.");
        if (bairro is null) erros.Add("Escolha o bairro.");

        return erros;
    }
}
