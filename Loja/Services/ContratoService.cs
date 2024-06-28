using System;
using System.Threading.Tasks;
using Loja.data;
using Loja.models;

namespace Loja.services
{
    public class ContratoService
    {
        private readonly LojaDbContext _dbContext;

        public ContratoService(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddContratoAsync(Contrato contrato)
        {
            _dbContext.Contratos.Add(contrato);
            await _dbContext.SaveChangesAsync();
        }
    }
}
