using System.Collections.Generic;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IUpdatesRepository : IRepository<Updates>
    {
        /// <summary>Bitácora del ticket, la más reciente primero.</summary>
        IEnumerable<Updates> GetByTicket(int idTicket);
    }

    public class UpdatesRepository : Repository<Updates>, IUpdatesRepository
    {
        public UpdatesRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Updates> GetByTicket(int idTicket)
        {
            return DbSet.AsNoTracking()
                .Where(u => u.id_ticket == idTicket)
                .OrderByDescending(u => u.fecha)
                .ThenByDescending(u => u.id)
                .ToList();
        }
    }
}
