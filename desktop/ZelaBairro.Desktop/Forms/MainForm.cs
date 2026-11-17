using ZelaBairro.Desktop.Models;
using ZelaBairro.Desktop.Services;

namespace ZelaBairro.Desktop.Forms;

/// <summary>Terminal de triagem da subprefeitura: lista, filtra, prioriza e muda o status das ocorrências.</summary>
public class MainForm : Form
{
    private readonly Configuracao _config;
    private readonly ApiClient _api;
    private readonly RegistroLocal _registro;

    private List<Ocorrencia> _ocorrencias = new();
    private List<Categoria> _categorias = new();
    private List<Bairro> _bairros = new();
    private bool _modoOffline;

    // Filtros e resumo
    private readonly ComboBox _cbBairro = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170, Font = Estilo.Normal };
    private readonly ComboBox _cbStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Font = Estilo.Normal };
    private readonly Label _lblTotal = NovoNumero(), _lblAbertas = NovoNumero(), _lblAtendimento = NovoNumero(),
        _lblAtrasadas = NovoNumero(), _lblNoPrazo = NovoNumero();

    // Lista
    private readonly DataGridView _grid = new();

    // Detalhe
    private readonly Label _lblTitulo = new() { AutoSize = true, Font = Estilo.Subtitulo, MaximumSize = new Size(400, 0) };
    private readonly Label _lblInfo = new() { AutoSize = true, Font = Estilo.Normal, MaximumSize = new Size(400, 0) };
    private readonly PictureBox _foto = new() { Width = 400, Height = 200, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Gainsboro, Visible = false };
    private readonly Label _lblSugestao = new() { AutoSize = true, Font = Estilo.Normal, MaximumSize = new Size(400, 0), ForeColor = Estilo.Alerta };
    private readonly ComboBox _cbPrioridade = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130, Font = Estilo.Normal };
    private readonly Button _btnAtender = Estilo.Botao("Iniciar atendimento", primario: true);
    private readonly Button _btnResolver = Estilo.Botao("Marcar resolvida", primario: true);
    private readonly Button _btnCancelar = Estilo.Botao("Cancelar");
    private readonly Button _btnPriorizar = Estilo.Botao("Aplicar prioridade");
    private readonly Button _btnUsarSugestao = Estilo.Botao("Usar previsão do modelo");
    private readonly Panel _painelDetalhe = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12), BackColor = Color.White };

    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private SugestaoPrioridade? _sugestaoAtual;
    private PrevisaoModelo? _previsaoAtual;
    private readonly ModeloPrioridade? _modelo =
        ModeloPrioridade.Carregar(Path.Combine(AppContext.BaseDirectory, "modelo_prioridade.json"));

    /// <summary>O que o botão "Usar previsão" aplica: o modelo de ML, ou a regra se o modelo não carregou.</summary>
    private Prioridade? PrioridadeSugerida => _previsaoAtual?.Prioridade ?? _sugestaoAtual?.Prioridade;

    public MainForm(Configuracao config, ApiClient api, RegistroLocal registro)
    {
        _config = config;
        _api = api;
        _registro = registro;

        Text = "ZelaBairro — Terminal da Subprefeitura";
        MinimumSize = new Size(1100, 700);
        Size = new Size(1280, 780);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Estilo.Fundo;
        Font = Estilo.Normal;

        MontarTela();
        Load += async (_, _) => await CarregarTudoAsync();
    }

    // =====================================================================
    // Montagem da interface
    // =====================================================================
    private void MontarTela()
    {
        // ---- Cabeçalho ----
        var cabecalho = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 10, 12, 4), WrapContents = true };
        cabecalho.Controls.Add(new Label { Text = "ZelaBairro", Font = Estilo.Titulo, ForeColor = Estilo.Primaria, AutoSize = true, Margin = new Padding(0, 0, 24, 0) });
        cabecalho.Controls.Add(Estilo.Rotulo("Bairro:"));
        cabecalho.Controls.Add(_cbBairro);
        cabecalho.Controls.Add(Estilo.Rotulo("Status:"));
        cabecalho.Controls.Add(_cbStatus);

        var btnAtualizar = Estilo.Botao("⟳ Atualizar", primario: true);
        btnAtualizar.Click += async (_, _) => await CarregarOcorrenciasAsync();
        var btnNova = Estilo.Botao("+ Registrar (telefone/balcão)");
        btnNova.Click += async (_, _) => await AbrirNovaOcorrenciaAsync();
        var btnHistorico = Estilo.Botao("Histórico de atendimento");
        btnHistorico.Click += (_, _) => new HistoricoForm(_registro).ShowDialog(this);
        cabecalho.Controls.AddRange(new Control[] { btnAtualizar, btnNova, btnHistorico });

        _cbStatus.Items.Add("Todos");
        foreach (var s in Enum.GetValues<StatusOcorrencia>()) _cbStatus.Items.Add(s);
        _cbStatus.SelectedIndex = 1; // começa mostrando as abertas: a fila da triagem
        _cbStatus.Format += (_, e) => { if (e.ListItem is StatusOcorrencia s) e.Value = Estilo.Texto(s); };
        _cbStatus.SelectionChangeCommitted += async (_, _) => await CarregarOcorrenciasAsync();
        _cbBairro.SelectionChangeCommitted += async (_, _) => await CarregarOcorrenciasAsync();

        // ---- Indicadores ----
        var indicadores = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 0, 12, 8) };
        indicadores.Controls.Add(Indicador("Total", _lblTotal));
        indicadores.Controls.Add(Indicador("Abertas", _lblAbertas));
        indicadores.Controls.Add(Indicador("Em atendimento", _lblAtendimento));
        indicadores.Controls.Add(Indicador("Fora do prazo", _lblAtrasadas));
        indicadores.Controls.Add(Indicador("Resolvidas no prazo", _lblNoPrazo));

        // ---- Grade ----
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.Font = Estilo.Normal;
        _grid.RowTemplate.Height = 32;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Estilo.Primaria;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = Estilo.Negrito;
        _grid.ColumnHeadersHeight = 36;
        _grid.DefaultCellStyle.SelectionBackColor = Estilo.PrimariaClara;
        _grid.DefaultCellStyle.SelectionForeColor = Color.Black;

        _grid.Columns.Add(Coluna("Id", "Nº", 6));
        _grid.Columns.Add(Coluna("Titulo", "Problema", 30));
        _grid.Columns.Add(Coluna("Categoria", "Categoria", 18));
        _grid.Columns.Add(Coluna("Bairro", "Bairro", 12));
        _grid.Columns.Add(Coluna("Status", "Status", 12));
        _grid.Columns.Add(Coluna("Prioridade", "Prioridade", 10));
        _grid.Columns.Add(Coluna("Confirmacoes", "👍", 5));
        _grid.Columns.Add(Coluna("Aberta", "Aberta em", 13));
        _grid.SelectionChanged += async (_, _) => await MostrarDetalheAsync();

        // ---- Detalhe ----
        var pilha = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        pilha.Controls.Add(_foto);
        pilha.Controls.Add(_lblTitulo);
        pilha.Controls.Add(_lblInfo);
        pilha.Controls.Add(Estilo.Rotulo("Triagem", Estilo.Subtitulo));
        pilha.Controls.Add(_lblSugestao);

        var linhaPrioridade = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
        _cbPrioridade.Items.AddRange(new object[] { Prioridade.Baixa, Prioridade.Media, Prioridade.Alta });
        _cbPrioridade.Format += (_, e) => { if (e.ListItem is Prioridade p) e.Value = Estilo.Texto(p); };
        linhaPrioridade.Controls.AddRange(new Control[] { _cbPrioridade, _btnPriorizar, _btnUsarSugestao });
        pilha.Controls.Add(linhaPrioridade);

        pilha.Controls.Add(Estilo.Rotulo("Atendimento", Estilo.Subtitulo));
        var linhaStatus = new FlowLayoutPanel { AutoSize = true };
        linhaStatus.Controls.AddRange(new Control[] { _btnAtender, _btnResolver, _btnCancelar });
        pilha.Controls.Add(linhaStatus);
        _painelDetalhe.Controls.Add(pilha);

        _btnAtender.Click += async (_, _) => await MudarStatusAsync(StatusOcorrencia.EmAtendimento);
        _btnResolver.Click += async (_, _) => await MudarStatusAsync(StatusOcorrencia.Resolvida);
        _btnCancelar.Click += async (_, _) => await MudarStatusAsync(StatusOcorrencia.Cancelada);
        _btnPriorizar.Click += async (_, _) => await AplicarPrioridadeAsync(_cbPrioridade.SelectedItem as Prioridade?);
        _btnUsarSugestao.Click += async (_, _) => await AplicarPrioridadeAsync(PrioridadeSugerida);

        var divisao = new SplitContainer { Dock = DockStyle.Fill, SplitterWidth = 6, BackColor = Estilo.Fundo };
        divisao.Panel1.Controls.Add(_grid);
        divisao.Panel2.Controls.Add(_painelDetalhe);
        Shown += (_, _) => divisao.SplitterDistance = Math.Max(400, ClientSize.Width - 460);

        var rodape = new StatusStrip();
        rodape.Items.Add(_status);

        Controls.Add(divisao);
        Controls.Add(indicadores);
        Controls.Add(cabecalho);
        Controls.Add(rodape);

        LimparDetalhe();
    }

    private static Label NovoNumero() => new() { AutoSize = true, Font = Estilo.Numero, ForeColor = Estilo.Primaria, Text = "–" };

    private static Control Indicador(string titulo, Label numero)
    {
        var painel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(12, 6, 24, 6),
            Margin = new Padding(0, 0, 8, 0)
        };
        painel.Controls.Add(numero);
        painel.Controls.Add(new Label { Text = titulo, AutoSize = true, ForeColor = Color.DimGray });
        return painel;
    }

    private static DataGridViewTextBoxColumn Coluna(string nome, string cabecalho, int peso) => new()
    {
        Name = nome,
        HeaderText = cabecalho,
        FillWeight = peso,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    // =====================================================================
    // Carregamento de dados
    // =====================================================================
    private async Task CarregarTudoAsync()
    {
        DefinirStatus($"Conectando à API em {_api.Endereco}...");
        try
        {
            _categorias = await _api.ListarCategoriasAsync();
            _bairros = await _api.ListarBairrosAsync();

            _cbBairro.Items.Clear();
            _cbBairro.Items.Add("Todos os bairros");
            foreach (var b in _bairros) _cbBairro.Items.Add(b);
            _cbBairro.SelectedIndex = 0;

            await CarregarOcorrenciasAsync();
        }
        catch (ApiException e)
        {
            AtivarModoOffline(e.Message);
        }
    }

    private async Task CarregarOcorrenciasAsync()
    {
        var bairroId = (_cbBairro.SelectedItem as Bairro)?.Id;
        var status = _cbStatus.SelectedItem as StatusOcorrencia?;

        UseWaitCursor = true;
        try
        {
            _ocorrencias = await _api.ListarOcorrenciasAsync(bairroId, status);
            _modoOffline = false;
            _registro.SalvarCache(_ocorrencias);
            PreencherGrade();
            await AtualizarResumoAsync();
            DefinirStatus($"{_ocorrencias.Count} ocorrência(s) · Operador: {_config.Operador} · API: {_api.Endereco} · atualizado às {DateTime.Now:HH:mm:ss}");
        }
        catch (ApiException e)
        {
            AtivarModoOffline(e.Message);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task AtualizarResumoAsync()
    {
        var resumo = await _api.ObterResumoAsync();
        _lblTotal.Text = resumo.Total.ToString();
        _lblAbertas.Text = resumo.Abertas.ToString();
        _lblAtendimento.Text = resumo.EmAtendimento.ToString();
        _lblAtrasadas.Text = resumo.Atrasadas.ToString();
        _lblAtrasadas.ForeColor = resumo.Atrasadas > 0 ? Color.Firebrick : Estilo.Primaria;
        _lblNoPrazo.Text = $"{resumo.PercentualResolvidasNoPrazo:0.#}%";
    }

    /// <summary>Se a API cair, o operador ainda consulta a última lista salva (só leitura).</summary>
    private void AtivarModoOffline(string motivo)
    {
        var cache = _registro.LerCache();
        if (cache is null)
        {
            DefinirStatus("Sem conexão com a API.");
            MessageBox.Show(this, motivo, "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _modoOffline = true;
        _ocorrencias = cache;
        PreencherGrade();
        DefinirStatus($"MODO OFFLINE — exibindo cópia salva em {_registro.DataDoCache():dd/MM HH:mm}. Ações desativadas.");
        MessageBox.Show(this, motivo + "\n\nExibindo a última lista salva (somente consulta).",
            "Modo offline", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void PreencherGrade()
    {
        var selecionado = OcorrenciaSelecionada()?.Id;
        _grid.Rows.Clear();

        // Ordem da fila: atrasadas primeiro, depois prioridade, depois as mais antigas.
        var fila = _ocorrencias
            .OrderByDescending(o => o.Atrasada)
            .ThenByDescending(o => o.Prioridade)
            .ThenBy(o => o.CriadaEm);

        foreach (var o in fila)
        {
            var indice = _grid.Rows.Add(o.Id, o.Titulo, o.Categoria, o.Bairro,
                Estilo.Texto(o.Status), Estilo.Texto(o.Prioridade), o.Confirmacoes, Estilo.Data(o.CriadaEm));
            var linha = _grid.Rows[indice];
            linha.Tag = o;
            if (o.Atrasada) linha.DefaultCellStyle.BackColor = Estilo.Atraso;
            if (o.Prioridade == Prioridade.Alta) linha.DefaultCellStyle.Font = Estilo.Negrito;
            if (o.Id == selecionado) linha.Selected = true;
        }

        if (_grid.Rows.Count == 0) LimparDetalhe("Nenhuma ocorrência com esses filtros.");
                else _ = MostrarDetalheAsync();
    }

    // =====================================================================
    // Detalhe e triagem
    // =====================================================================
    private Ocorrencia? OcorrenciaSelecionada() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as Ocorrencia : null;

    private void LimparDetalhe(string mensagem = "Selecione uma ocorrência na lista.")
    {
        _foto.Visible = false;
        _lblTitulo.Text = mensagem;
        _lblInfo.Text = "";
        _lblSugestao.Text = "";
        _sugestaoAtual = null;
        _previsaoAtual = null;
        foreach (var b in new[] { _btnAtender, _btnResolver, _btnCancelar, _btnPriorizar, _btnUsarSugestao }) b.Enabled = false;
        _cbPrioridade.Enabled = false;
    }

    private async Task MostrarDetalheAsync()
    {
        var o = OcorrenciaSelecionada();
        if (o is null) { LimparDetalhe(); return; }

        var categoria = _categorias.FirstOrDefault(c => c.Id == o.CategoriaId);
        var prazo = categoria?.PrazoAtendimentoDias ?? 0;
        var agora = DateTime.UtcNow;

        _lblTitulo.Text = $"#{o.Id} — {o.Titulo}";
        _lblInfo.Text =
            (string.IsNullOrWhiteSpace(o.Descricao) ? "" : o.Descricao + "\n\n") +
            $"Categoria: {o.Categoria} (prazo {prazo} dias)\n" +
            $"Bairro: {o.Bairro}\n" +
            (o.Endereco is null ? "" : $"Endereço: {o.Endereco}\n") +
            (o.Latitude == 0 && o.Longitude == 0 ? "" : $"Coordenadas: {o.Latitude:0.00000}, {o.Longitude:0.00000}\n") +
            $"Origem: {o.Origem}  ·  Confirmações: {o.Confirmacoes}\n" +
            $"Aberta em: {Estilo.Data(o.CriadaEm)} ({o.DiasEmAberto(agora)} dia(s))\n" +
            (o.ResolvidaEm is null ? "" : $"Resolvida em: {Estilo.Data(o.ResolvidaEm.Value)}\n") +
            $"Status: {Estilo.Texto(o.Status)}  ·  Prioridade: {Estilo.Texto(o.Prioridade)}" +
            (o.Atrasada ? "\n⚠ FORA DO PRAZO" : "");

        // Triagem: previsão do modelo de ML (principal) + regra de pontos (referência)
        if (o.PodePriorizar)
        {
            _sugestaoAtual = Priorizador.Sugerir(o, prazo, agora);
            _previsaoAtual = _modelo?.Prever(o, prazo, agora);

            var textoModelo = _previsaoAtual is null
                ? "Modelo de ML não encontrado (modelo_prioridade.json). Usando só a regra."
                : $"🤖 Previsão do modelo (ML v{_modelo!.Versao}): {Estilo.Texto(_previsaoAtual.Prioridade)} — {_previsaoAtual.Confianca:P0} de confiança" +
                  (_previsaoAtual.Fatores.Count > 0 ? $"\n   Pesou: {string.Join(", ", _previsaoAtual.Fatores)}" : "");

            _lblSugestao.Text = textoModelo +
                $"\n\nRegra de pontos (referência): {Estilo.Texto(_sugestaoAtual.Prioridade)} ({_sugestaoAtual.Pontos} pontos)\n" +
                string.Join("\n", _sugestaoAtual.Motivos.Select(m => "  " + m));

            _cbPrioridade.SelectedItem = o.Prioridade == Prioridade.NaoDefinida ? PrioridadeSugerida : o.Prioridade;
        }
        else
        {
            _sugestaoAtual = null;
            _previsaoAtual = null;
            _lblSugestao.Text = "Ocorrência encerrada.";
        }

        // Só habilita o que a regra de negócio permite (e nada no modo offline)
        var online = !_modoOffline;
        _btnAtender.Enabled = online && o.PodeIniciarAtendimento;
        _btnResolver.Enabled = online && o.PodeResolver;
        _btnCancelar.Enabled = online && o.PodeCancelar;
        _btnPriorizar.Enabled = _cbPrioridade.Enabled = online && o.PodePriorizar;
        _btnUsarSugestao.Enabled = online && PrioridadeSugerida is not null;

        // Foto (carrega sem travar a tela)
        _foto.Image?.Dispose();
        _foto.Image = null;
        _foto.Visible = false;
        if (o.TemFoto && online)
        {
            var id = o.Id;
            var imagem = await _api.ObterFotoAsync(id);
            if (OcorrenciaSelecionada()?.Id == id && imagem is not null)
            {
                _foto.Image = imagem;
                _foto.Visible = true;
            }
            else
            {
                imagem?.Dispose();
            }
        }
    }

    // =====================================================================
    // Ações do operador
    // =====================================================================
    private async Task MudarStatusAsync(StatusOcorrencia novo)
    {
        var o = OcorrenciaSelecionada();
        if (o is null) return;

        if (novo == StatusOcorrencia.Cancelada &&
            MessageBox.Show(this, $"Cancelar a ocorrência #{o.Id}? Essa ação não pode ser desfeita.",
                "Confirmar cancelamento", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        await ExecutarAcaoAsync(o, () => _api.AlterarStatusAsync(o.Id, novo),
            "Status", $"{Estilo.Texto(o.Status)} → {Estilo.Texto(novo)}");
    }

    private async Task AplicarPrioridadeAsync(Prioridade? prioridade)
    {
        var o = OcorrenciaSelecionada();
        if (o is null || prioridade is null) return;

        var origem = _previsaoAtual is not null && prioridade == _previsaoAtual.Prioridade
            ? $" (previsão do modelo ML, {_previsaoAtual.Confianca:P0})"
            : _sugestaoAtual is not null && prioridade == _sugestaoAtual.Prioridade ? " (regra de pontos)" : "";
        await ExecutarAcaoAsync(o, () => _api.DefinirPrioridadeAsync(o.Id, prioridade.Value),
            "Prioridade", $"{Estilo.Texto(o.Prioridade)} → {Estilo.Texto(prioridade.Value)}{origem}");
    }

    private async Task ExecutarAcaoAsync(Ocorrencia o, Func<Task<Ocorrencia>> acao, string tipo, string detalhe)
    {
        UseWaitCursor = true;
        try
        {
            await acao();
            _registro.RegistrarAcao(_config.Operador, o.Id, tipo, detalhe);
            DefinirStatus($"Ocorrência #{o.Id}: {tipo.ToLower()} {detalhe}");
            await CarregarOcorrenciasAsync();
        }
        catch (ApiException e)
        {
            MessageBox.Show(this, e.Message, "Não foi possível concluir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task AbrirNovaOcorrenciaAsync()
    {
        if (_modoOffline)
        {
            MessageBox.Show(this, "Sem conexão com a API. Tente atualizar primeiro.", "Modo offline");
            return;
        }

        using var form = new NovaOcorrenciaForm(_api, _categorias, _bairros);
        if (form.ShowDialog(this) != DialogResult.OK || form.Criada is null) return;

        _registro.RegistrarAcao(_config.Operador, form.Criada.Id, "Cadastro", $"Registrada no terminal: {form.Criada.Titulo}");
        _cbStatus.SelectedIndex = 1;
        await CarregarOcorrenciasAsync();
    }

    private void DefinirStatus(string texto) => _status.Text = texto;
}
