using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IVehiculoRepository : IRepository<Vehiculo>
    {
        /// <summary>Busca por placa, marca, modelo o nombre del cliente. Incluye Cliente y VehiculoSpecs.</summary>
        IEnumerable<Vehiculo> Buscar(string termino);

        /// <summary>Vehículo con Cliente, Specs y Tickets. Sin tracking, para vistas.</summary>
        Vehiculo GetConDetalle(int id);

        IEnumerable<Vehiculo> GetByCliente(int idCliente);

        Vehiculo GetByPlaca(string placa);

        /// <summary>True si la placa ya está registrada en otro vehículo.</summary>
        bool PlacaExiste(string placa, int? excluirId = null);
    }

    public class VehiculoRepository : Repository<Vehiculo>, IVehiculoRepository
    {
        public VehiculoRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Vehiculo> Buscar(string termino)
        {
            var query = DbSet.AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.VehiculoSpecs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.Trim();
                query = query.Where(v => v.placa.Contains(termino)
                                      || v.VehiculoSpecs.marca.Contains(termino)
                                      || v.VehiculoSpecs.modelo.Contains(termino)
                                      || v.Cliente.nombre.Contains(termino));
            }

            return query.OrderBy(v => v.placa).ToList();
        }

        public Vehiculo GetConDetalle(int id)
        {
            return DbSet.AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.VehiculoSpecs)
                .Include(v => v.Ticket)
                .FirstOrDefault(v => v.id == id);
        }

        public IEnumerable<Vehiculo> GetByCliente(int idCliente)
        {
            return DbSet.AsNoTracking()
                .Include(v => v.VehiculoSpecs)
                .Where(v => v.id_cliente == idCliente)
                .OrderBy(v => v.placa)
                .ToList();
        }

        public Vehiculo GetByPlaca(string placa)
        {
            placa = (placa ?? "").Trim();
            return DbSet.AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.VehiculoSpecs)
                .FirstOrDefault(v => v.placa == placa);
        }

        public bool PlacaExiste(string placa, int? excluirId = null)
        {
            placa = (placa ?? "").Trim();
            return DbSet.AsNoTracking().Any(v => v.placa == placa
                                              && (!excluirId.HasValue || v.id != excluirId.Value));
        }
    }
}
