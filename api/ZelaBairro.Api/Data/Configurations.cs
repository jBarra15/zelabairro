using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZelaBairro.Api.Domain;

namespace ZelaBairro.Api.Data;

public class BairroConfig : IEntityTypeConfiguration<Bairro>
{
    public void Configure(EntityTypeBuilder<Bairro> b)
    {
        b.ToTable("ZB_BAIRRO");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        b.Property(x => x.Cidade).HasMaxLength(80).IsRequired();

        // Dados de referência (troque pelos bairros reais do seu território)
        b.HasData(
            new { Id = 1, Nome = "Bela Vista", Cidade = "São Paulo" },
            new { Id = 2, Nome = "Consolação", Cidade = "São Paulo" },
            new { Id = 3, Nome = "Liberdade", Cidade = "São Paulo" },
            new { Id = 4, Nome = "Paraíso", Cidade = "São Paulo" },
            new { Id = 5, Nome = "Vila Mariana", Cidade = "São Paulo" });
    }
}

public class CategoriaConfig : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        b.ToTable("ZB_CATEGORIA");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(60).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(200);
        b.HasIndex(x => x.Nome).IsUnique();

        b.HasData(
            new { Id = 1, Nome = "Lixo e descarte irregular", Descricao = "Entulho, lixo acumulado, lixeira cheia", PrazoAtendimentoDias = 3 },
            new { Id = 2, Nome = "Iluminação pública", Descricao = "Poste apagado ou piscando", PrazoAtendimentoDias = 5 },
            new { Id = 3, Nome = "Buraco na via", Descricao = "Buracos e afundamentos no asfalto", PrazoAtendimentoDias = 10 },
            new { Id = 4, Nome = "Calçada danificada", Descricao = "Calçada quebrada, sem rampa ou obstruída", PrazoAtendimentoDias = 15 },
            new { Id = 5, Nome = "Árvore e poda", Descricao = "Galho caindo, árvore com risco de queda", PrazoAtendimentoDias = 7 });
    }
}

public class MoradorConfig : IEntityTypeConfiguration<Morador>
{
    public void Configure(EntityTypeBuilder<Morador> b)
    {
        b.ToTable("ZB_MORADOR");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        b.Property(x => x.Email).HasMaxLength(120).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();

        b.HasOne(x => x.Bairro)
            .WithMany()
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OcorrenciaConfig : IEntityTypeConfiguration<Ocorrencia>
{
    public void Configure(EntityTypeBuilder<Ocorrencia> b)
    {
        b.ToTable("ZB_OCORRENCIA");
        b.HasKey(x => x.Id);

        b.Property(x => x.Titulo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(500);
        b.Property(x => x.Endereco).HasMaxLength(200);
        b.Property(x => x.FotoArquivo).HasMaxLength(100);

        // Enums gravados como texto: legível no SQL Developer e seguro se a ordem do enum mudar
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Prioridade).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Origem).HasConversion<string>().HasMaxLength(20);

        b.Ignore(x => x.EstaEncerrada);

        b.HasOne(x => x.Categoria)
            .WithMany()
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Bairro)
            .WithMany(x => x.Ocorrencias)
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Morador)
            .WithMany()
            .HasForeignKey(x => x.MoradorId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.BairroId);
    }
}
