using ZelaBairro.Desktop.Models;
using ZelaBairro.Desktop.Services;

namespace ZelaBairro.Desktop.Forms;

/// <summary>
/// Cadastro feito pelo operador: para o cidadão que liga ou vai ao balcão
/// e não usa o app (idosos, quem não tem smartphone).
/// </summary>
public class NovaOcorrenciaForm : Form
{
    private readonly ApiClient _api;

    private readonly ComboBox _cbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 380 };
    private readonly ComboBox _cbBairro = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 380 };
    private readonly TextBox _txtTitulo = new() { Width = 380, MaxLength = 100 };
    private readonly TextBox _txtDescricao = new() { Width = 380, Height = 90, Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _txtEndereco = new() { Width = 380, MaxLength = 200 };
    private readonly Label _lblErros = new() { AutoSize = true, ForeColor = Color.Firebrick, MaximumSize = new Size(380, 0) };
    private readonly Button _btnSalvar = Estilo.Botao("Registrar ocorrência", primario: true);

    public Ocorrencia? Criada { get; private set; }

    public NovaOcorrenciaForm(ApiClient api, IEnumerable<Categoria> categorias, IEnumerable<Bairro> bairros)
    {
        _api = api;

        Text = "Registrar ocorrência (atendimento por telefone ou balcão)";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Font = Estilo.Normal;
        BackColor = Color.White;

        _cbCategoria.Items.AddRange(categorias.Cast<object>().ToArray());
        _cbBairro.Items.AddRange(bairros.Cast<object>().ToArray());

        var pilha = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(16)
        };
        pilha.Controls.Add(Estilo.Rotulo("Tipo de problema *"));
        pilha.Controls.Add(_cbCategoria);
        pilha.Controls.Add(Estilo.Rotulo("Bairro *"));
        pilha.Controls.Add(_cbBairro);
        pilha.Controls.Add(Estilo.Rotulo("Título *  (ex.: Poste apagado na esquina)"));
        pilha.Controls.Add(_txtTitulo);
        pilha.Controls.Add(Estilo.Rotulo("Detalhes informados pelo cidadão"));
        pilha.Controls.Add(_txtDescricao);
        pilha.Controls.Add(Estilo.Rotulo("Endereço ou ponto de referência *"));
        pilha.Controls.Add(_txtEndereco);
        pilha.Controls.Add(_lblErros);

        var botoes = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        var btnCancelar = Estilo.Botao("Cancelar");
        btnCancelar.DialogResult = DialogResult.Cancel;
        botoes.Controls.Add(_btnSalvar);
        botoes.Controls.Add(btnCancelar);
        pilha.Controls.Add(botoes);

        Controls.Add(pilha);
        AcceptButton = _btnSalvar;
        CancelButton = btnCancelar;
        _btnSalvar.Click += async (_, _) => await SalvarAsync();
    }

    private async Task SalvarAsync()
    {
        var erros = ValidacaoOcorrencia.Validar(_txtTitulo.Text, _txtDescricao.Text, _txtEndereco.Text,
            _cbCategoria.SelectedItem, _cbBairro.SelectedItem);
        if (erros.Count > 0)
        {
            _lblErros.Text = "• " + string.Join("\n• ", erros);
            return;
        }

        var categoria = (Categoria)_cbCategoria.SelectedItem!;
        var bairro = (Bairro)_cbBairro.SelectedItem!;

        _btnSalvar.Enabled = false;
        _lblErros.Text = "";
        UseWaitCursor = true;
        try
        {
            Criada = await _api.CriarOcorrenciaAsync(new
            {
                titulo = _txtTitulo.Text.Trim(),
                descricao = _txtDescricao.Text.Trim(),
                categoriaId = categoria.Id,
                bairroId = bairro.Id,
                moradorId = (int?)null,
                latitude = 0.0,
                longitude = 0.0,
                endereco = _txtEndereco.Text.Trim(),
                origem = "Terminal"
            });

            MessageBox.Show(this, $"Ocorrência #{Criada.Id} registrada. Informe esse número ao cidadão para acompanhamento.",
                "Registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException e)
        {
            _lblErros.Text = e.Message;
            _btnSalvar.Enabled = true;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }
}
