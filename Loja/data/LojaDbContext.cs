using Loja.models;
using Microsoft.EntityFrameworkCore;

namespace Loja.data
{
    public class LojaDbContext : DbContext
    {
        public LojaDbContext(DbContextOptions<LojaDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Contrato> Contratos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Servico)
                .WithMany()
                .HasForeignKey(c => c.ServicoId);
        }
    }
}