namespace ZelaBairro.Desktop.Forms;

/// <summary>Cores e fontes do terminal (mesma identidade do app mobile).</summary>
internal static class Estilo
{
    public static readonly Color Primaria = Color.FromArgb(11, 93, 59);
    public static readonly Color PrimariaClara = Color.FromArgb(227, 242, 234);
    public static readonly Color Fundo = Color.FromArgb(245, 247, 246);
    public static readonly Color Atraso = Color.FromArgb(253, 236, 234);
    public static readonly Color Alerta = Color.FromArgb(122, 79, 0);

    public static readonly Font Normal = new("Segoe UI", 10F);
    public static readonly Font Negrito = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font Titulo = new("Segoe UI", 16F, FontStyle.Bold);
    public static readonly Font Subtitulo = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font Numero = new("Segoe UI", 18F, FontStyle.Bold);

    public static Button Botao(string texto, bool primario = false)
    {
        var botao = new Button
        {
            Text = texto,
            AutoSize = true,
            MinimumSize = new Size(120, 36),
            Padding = new Padding(8, 2, 8, 2),
            Margin = new Padding(0, 0, 8, 8),
            Font = Negrito,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = primario ? Primaria : Color.White,
            ForeColor = primario ? Color.White : Primaria
        };
        botao.FlatAppearance.BorderColor = Primaria;
        botao.EnabledChanged += (_, _) =>
        {
            botao.BackColor = !botao.Enabled ? Color.Gainsboro : primario ? Primaria : Color.White;
            botao.ForeColor = !botao.Enabled ? Color.Gray : primario ? Color.White : Primaria;
            botao.FlatAppearance.BorderColor = botao.Enabled ? Primaria : Color.Silver;
        };
        return botao;
    }

    public static Label Rotulo(string texto, Font? fonte = null) => new()
    {
        Text = texto,
        AutoSize = true,
        Font = fonte ?? Normal,
        Margin = new Padding(0, 4, 0, 2)
    };

    public static string Texto(Models.StatusOcorrencia status) => status switch
    {
        Models.StatusOcorrencia.Aberta => "Aberta",
        Models.StatusOcorrencia.EmAtendimento => "Em atendimento",
        Models.StatusOcorrencia.Resolvida => "Resolvida",
        _ => "Cancelada"
    };

    public static string Texto(Models.Prioridade prioridade) => prioridade switch
    {
        Models.Prioridade.Alta => "Alta",
        Models.Prioridade.Media => "Média",
        Models.Prioridade.Baixa => "Baixa",
        _ => "Não definida"
    };

    /// <summary>A API grava em UTC sem fuso; converte para o horário de Brasília.</summary>
    public static string Data(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}
