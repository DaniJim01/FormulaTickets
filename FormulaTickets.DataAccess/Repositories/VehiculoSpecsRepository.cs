using System.Collections.Generic;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IVehiculoSpecsRepository : IRepository<VehiculoSpecs>
    {
        /// <summary>Filtra por marca/modelo (texto) y opcionalmente por año.</summary>
        IEnumerable<VehiculoSpecs> Buscar(string termino, int? anio);

        /// <summary>True si ya existe la misma marca+modelo+año (ignorando el id indicado).</summary>
        bool Existe(string marca, string modelo, int anio, int? excluirId = null);

        /// <summary>True si algún vehículo usa estas specs (no se pueden borrar).</summary>
        bool TieneVehiculos(int id);
    }

    public class VehiculoSpecsRepository : Repository<VehiculoSpecs>, IVehiculoSpecsRepository
    {
        public VehiculoSpecsRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<VehiculoSpecs> Buscar(string termino, int? anio)
        {
            var query = DbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.Trim();
                query = query.Where(s => s.marca.Contains(termino) || s.modelo.Contains(termino));
            }

            if (anio.HasValue)
                query = query.Where(s => s.anio == anio.Value);

            return query.OrderBy(s => s.marca).ThenBy(s => s.modelo).ThenByDescending(s => s.anio).ToList();
        }

        public bool Existe(string marca, string modelo, int anio, int? excluirId = null)
        {
            marca = (marca ?? "").Trim();
            modelo = (modelo ?? "").Trim();

            return DbSet.AsNoTracking().Any(s => s.marca == marca
                                              && s.modelo == modelo
                                              && s.anio == anio
                                              && (!excluirId.HasValue || s.id != excluirId.Value));
        }

        public bool TieneVehiculos(int id)
        {
            return Context.Vehiculo.Any(v => v.id_vehiculospecs == id);
        }
    }
}
