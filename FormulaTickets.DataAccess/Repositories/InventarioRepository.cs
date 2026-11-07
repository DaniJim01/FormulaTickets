using System.Collections.Generic;
using System.Linq;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IInventarioRepository : IRepository<Inventario>
    {
        /// <summary>
        /// Filtra por texto (nombre/especificación), categoría y, opcionalmente,
        /// por stock máximo (para el reporte de "stock bajo").
        /// </summary>
        IEnumerable<Inventario> Buscar(string termino, string categoria, int? stockMaximo);

        IEnumerable<string> GetCategorias();

        /// <summary>
        /// Productos cuya especificación técnica coincide con lo que las specs del vehículo
        /// requieren para esa categoría (aceite, filtro, llanta, caliper, pastilla, aro).
        /// </summary>
        IEnumerable<Inventario> GetCompatibles(VehiculoSpecs specs, bool soloConStock = true);

        /// <summary>True si el producto ya se usó en algún ticket (no se puede borrar).</summary>
        bool TieneMovimientos(int id);

        int ObtenerStock(int id);

        /// <summary>
        /// Descuenta stock de forma atómica en la base de datos (UPDATE condicional).
        /// Devuelve false, sin tocar nada, si no hay suficiente stock.
        /// Evita que dos tickets simultáneos dejen el stock en negativo.
        /// </summary>
        bool DescontarStock(int id, int cantidad);

        /// <summary>Suma stock de forma atómica.</summary>
        void ReponerStock(int id, int cantidad);
    }

    public class InventarioRepository : Repository<Inventario>, IInventarioRepository
    {
        public InventarioRepository(FormulaTicketsDBEntities context) : base(context) { }

        public IEnumerable<Inventario> Buscar(string termino, string categoria, int? stockMaximo)
        {
            var query = DbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.Trim();
                query = query.Where(i => i.nombre_producto.Contains(termino)
                                      || i.especificacion_tecnica.Contains(termino));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(i => i.categoria == categoria);

            if (stockMaximo.HasValue)
            {
                int max = stockMaximo.Value;
                query = query.Where(i => (i.stock ?? 0) <= max);
            }

            return query.OrderBy(i => i.categoria).ThenBy(i => i.nombre_producto).ToList();
        }

        public IEnumerable<string> GetCategorias()
        {
            return DbSet.AsNoTracking().Select(i => i.categoria).Distinct().OrderBy(c => c).ToList();
        }

        public IEnumerable<Inventario> GetCompatibles(VehiculoSpecs specs, bool soloConStock = true)
        {
            if (specs == null) return new List<Inventario>();

            var categorias = CategoriaInventario.Todas;
            var query = DbSet.AsNoTracking().Where(i => categorias.Contains(i.categoria));

            if (soloConStock)
                query = query.Where(i => (i.stock ?? 0) > 0);

            // El cruce contra las specs se hace en memoria: son pocas filas y cada
            // categoría apunta a una columna distinta de VehiculoSpecs.
            return query.ToList()
                .Where(i =>
                {
                    var requerida = CategoriaInventario.RequeridaPor(i.categoria, specs);
                    return !string.IsNullOrWhiteSpace(requerida)
                           && CategoriaInventario.Equivalentes(requerida, i.especificacion_tecnica);
                })
                .OrderBy(i => i.categoria)
                .ThenBy(i => i.nombre_producto)
                .ToList();
        }

        public bool TieneMovimientos(int id)
        {
            return Context.Detalle_Ticket.Any(d => d.id_inventario == id);
        }

        public int ObtenerStock(int id)
        {
            return DbSet.AsNoTracking().Where(i => i.id == id).Select(i => i.stock).FirstOrDefault() ?? 0;
        }

        public bool DescontarStock(int id, int cantidad)
        {
            int filas = Context.Database.ExecuteSqlCommand(
                "UPDATE Inventario SET stock = stock - @p0 WHERE id = @p1 AND ISNULL(stock, 0) >= @p0",
                cantidad, id);
            return filas > 0;
        }

        public void ReponerStock(int id, int cantidad)
        {
            Context.Database.ExecuteSqlCommand(
                "UPDATE Inventario SET stock = ISNULL(stock, 0) + @p0 WHERE id = @p1",
                cantidad, id);
        }
    }
}
