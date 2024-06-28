using Microsoft.EntityFrameworkCore;
using Loja.models;

namespace Loja.data
{
    public class LojaDbContext : DbContext
    {
        public LojaDbContext(DbContextOptions<LojaDbContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Servico> Servicos { get; set; }

        public DbSet<Contrato> Contratos { get; set; }
    }
}