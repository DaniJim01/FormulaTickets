using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        /// <summary>
        /// Lista tickets con Vehículo, Cliente y Specs. Filtra por estado y por texto
        /// (número de ticket, placa o nombre del cliente). Más recientes primero.
        /// </summary>
        IEnumerable<Ticket> Buscar(string estado, string termino);

        /// <summary>Ticket completo (vehículo, cliente, specs, updates, repuestos con inventario). Sin tracking.</summary>
        Ticket GetDetalle(int id);

        IEnumerable<Ticket> GetByVehiculo(int idVehiculo);

        /// <summary>Cantidad de tickets por estado (para el dashboard/filtros).</summary>
        IDictionary<string, int> ContarPorEstado();

        int ContarPorVehiculo(int idVehiculo);
        int ContarPorCliente(int idCliente);
    }

    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Ticket> Buscar(string estado, string termino)
        {
            var query = DbSet.AsNoTracking()
                .Include(t => t.Vehiculo.Cliente)
                .Include(t => t.Vehiculo.VehiculoSpecs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(t => t.estado == estado);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.Trim();
                int numero;
                bool esNumero = int.TryParse(termino.TrimStart('#'), out numero);

                query = query.Where(t => (esNumero && t.id == numero)
                                      || t.Vehiculo.placa.Contains(termino)
                                      || t.Vehiculo.Cliente.nombre.Contains(termino));
            }

            return query.OrderByDescending(t => t.fecha_creacion).ThenByDescending(t => t.id).ToList();
        }

        public Ticket GetDetalle(int id)
        {
            return DbSet.AsNoTracking()
                .Include(t => t.Vehiculo.Cliente)
                .Include(t => t.Vehiculo.VehiculoSpecs)
                .Include(t => t.Updates)
                .Include(t => t.Detalle_Ticket.Select(d => d.Inventario))
                .FirstOrDefault(t => t.id == id);
        }

        public IEnumerable<Ticket> GetByVehiculo(int idVehiculo)
        {
            return DbSet.AsNoTracking()
                .Where(t => t.id_vehiculo == idVehiculo)
                .OrderByDescending(t => t.fecha_creacion)
                .ToList();
        }

        public IDictionary<string, int> ContarPorEstado()
        {
            return DbSet.AsNoTracking()
                .GroupBy(t => t.estado)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToList()
                .ToDictionary(x => x.Estado, x => x.Total);
        }

        public int ContarPorVehiculo(int idVehiculo)
        {
            return DbSet.Count(t => t.id_vehiculo == idVehiculo);
        }

        public int ContarPorCliente(int idCliente)
        {
            return DbSet.Count(t => t.Vehiculo.id_cliente == idCliente);
        }
    }
}
