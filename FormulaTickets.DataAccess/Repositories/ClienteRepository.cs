using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        /// <summary>Busca por nombre o teléfono. Sin término devuelve todos.</summary>
        IEnumerable<Cliente> Buscar(string termino);

        /// <summary>Cliente con direcciones y vehículos (con sus specs). Sin tracking, para vistas.</summary>
        Cliente GetCompleto(int id);
    }

    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Cliente> Buscar(string termino)
        {
            var query = DbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.Trim();
                query = query.Where(c => c.nombre.Contains(termino)
                                      || c.telefono_1.Contains(termino)
                                      || c.telefono_2.Contains(termino));
            }

            return query.OrderBy(c => c.nombre).ToList();
        }

        public Cliente GetCompleto(int id)
        {
            return DbSet.AsNoTracking()
                .Include(c => c.Direccion)
                .Include(c => c.Vehiculo.Select(v => v.VehiculoSpecs))
                .FirstOrDefault(c => c.id == id);
        }
    }
}
