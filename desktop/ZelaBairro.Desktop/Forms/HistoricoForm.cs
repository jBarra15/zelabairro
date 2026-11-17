using System.Diagnostics;
using ZelaBairro.Desktop.Services;

namespace ZelaBairro.Desktop.Forms;

/// <summary>Histórico local de tudo o que foi feito neste terminal (persistido em JSON).</summary>
public class HistoricoForm : Form
{
    public HistoricoForm(RegistroLocal registro)
    {
        Text = "Histórico de atendimento deste terminal";
        Size = new Size(900, 500);
        StartPosition = FormStartPosition.CenterParent;
        Font = Estilo.Normal;

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        grid.Columns.Add("Data", "Data/hora");
        grid.Columns.Add("Operador", "Operador");
        grid.Columns.Add("Ocorrencia", "Nº");
        grid.Columns.Add("Acao", "Ação");
        grid.Columns.Add("Detalhe", "Detalhe");
        grid.Columns["Detalhe"]!.FillWeight = 250;
        grid.Columns["Ocorrencia"]!.FillWeight = 40;

        var historico = registro.LerHistorico();
        foreach (var r in historico.OrderByDescending(r => r.DataHora))
            grid.Rows.Add(r.DataHora.ToString("dd/MM/yyyy HH:mm:ss"), r.Operador, r.OcorrenciaId, r.Acao, r.Detalhe);

        var rodape = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
        rodape.Controls.Add(Estilo.Rotulo($"{historico.Count} registro(s) salvos em {registro.Pasta}"));
        var btnPasta = Estilo.Botao("Abrir pasta");
        btnPasta.Click += (_, _) => Process.Start(new ProcessStartInfo("explorer.exe", registro.Pasta) { UseShellExecute = true });
        rodape.Controls.Add(btnPasta);

        Controls.Add(grid);
        Controls.Add(rodape);
    }
}
