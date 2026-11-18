using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using ZelaBairro.Api.Data;

namespace ZelaBairro.Api.Controllers;

/// <summary>
/// Verificação de saúde: mostra em qual servidor a API está rodando e se o banco responde.
/// Usado pelo painel web, pelo monitoramento e no teste de falha.
/// </summary>
[ApiController]
[Route("api/saude")]
public class SaudeController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        bool bancoOk;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bancoOk = await db.Database.CanConnectAsync(cts.Token);
        }
        catch
        {
            bancoOk = false;
        }

        var resposta = new
        {
            status = bancoOk ? "ok" : "degradado",
            servidor = Environment.MachineName,
            ambiente = env.EnvironmentName,
            sistemaOperacional = RuntimeInformation.OSDescription,
            runtime = RuntimeInformation.FrameworkDescription,
            processoId = Environment.ProcessId,
            iniciadoEm = System.Diagnostics.Process.GetCurrentProcess().StartTime.ToString("dd/MM/yyyy HH:mm:ss"),
            horaServidor = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            banco = bancoOk ? "conectado" : "sem conexão"
        };

        return bancoOk ? Ok(resposta) : StatusCode(StatusCodes.Status503ServiceUnavailable, resposta);
    }
}
