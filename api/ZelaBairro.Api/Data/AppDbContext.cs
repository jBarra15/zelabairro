using Microsoft.EntityFrameworkCore;
using ZelaBairro.Api.Domain;

namespace ZelaBairro.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Morador> Moradores => Set<Morador>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
