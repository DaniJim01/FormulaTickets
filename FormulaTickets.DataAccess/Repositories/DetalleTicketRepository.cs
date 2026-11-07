using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IDetalleTicketRepository : IRepository<Detalle_Ticket>
    {
        /// <summary>Repuestos del ticket, con el producto de inventario. Sin tracking.</summary>
        IEnumerable<Detalle_Ticket> GetByTicket(int idTicket);

        /// <summary>Línea existente del mismo producto en el ticket (con tracking, para sumarle cantidad).</summary>
        Detalle_Ticket GetPorTicketEInventario(int idTicket, int idInventario);
    }

    public class DetalleTicketRepository : Repository<Detalle_Ticket>, IDetalleTicketRepository
    {
        public DetalleTicketRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Detalle_Ticket> GetByTicket(int idTicket)
        {
            return DbSet.AsNoTracking()
                .Include(d => d.Inventario)
                .Where(d => d.id_ticket == idTicket)
                .ToList();
        }

        public Detalle_Ticket GetPorTicketEInventario(int idTicket, int idInventario)
        {
            return DbSet.FirstOrDefault(d => d.id_ticket == idTicket && d.id_inventario == idInventario);
        }
    }
}
