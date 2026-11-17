using ZelaBairro.Desktop.Forms;
using ZelaBairro.Desktop.Services;

namespace ZelaBairro.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var config = Configuracao.Carregar();
        var api = new ApiClient(config.ApiUrl);
        var registro = new RegistroLocal();

        Application.Run(new MainForm(config, api, registro));
    }
}
