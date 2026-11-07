using System.Collections.Generic;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IDireccionRepository : IRepository<Direccion>
    {
        IEnumerable<Direccion> GetByCliente(int idCliente);
    }

    public class DireccionRepository : Repository<Direccion>, IDireccionRepository
    {
        public DireccionRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Direccion> GetByCliente(int idCliente)
        {
            return DbSet.AsNoTracking().Where(d => d.id_cliente == idCliente).ToList();
        }
    }
}
