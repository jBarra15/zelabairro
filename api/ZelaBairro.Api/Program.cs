using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;
using ZelaBairro.Api.Data;
using ZelaBairro.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// A senha do banco nunca fica no código nem no appsettings: vem dos user-secrets (dev)
// ou de variável de ambiente ConnectionStrings__Oracle (servidor/IIS).
builder.Configuration.AddUserSecrets<Program>(optional: true);

var connectionString = builder.Configuration.GetConnectionString("Oracle");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'Oracle' não configurada. Veja o README.");

// O servidor da FIAP é Oracle 19: avisamos o driver para gerar SQL compatível.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString, oracle =>
        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Regras de negócio violadas viram 400 com mensagem clara; o resto vira 500 sem vazar detalhes internos.
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var erro = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, mensagem) = erro is DomainException
        ? (StatusCodes.Status400BadRequest, erro.Message)
        : (StatusCodes.Status500InternalServerError, "Erro interno. Tente novamente mais tarde.");

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new { erro = mensagem });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Painel web (wwwroot/index.html) servido pela própria API: é a "aplicação WEB" hospedada no IIS.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();
