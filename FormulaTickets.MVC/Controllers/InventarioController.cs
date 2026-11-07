using System.Linq;
using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    public class InventarioController : BaseController
    {
        private const string CamposEditables = "nombre_producto,categoria,especificacion_tecnica,stock,precio";
        private const int UmbralStockBajo = 5;

        public InventarioController() { }
        public InventarioController(IUnitOfWork uow) : base(uow) { }

        // GET: Inventario?q=5W-30&categoria=Aceite&stockBajo=true
        public ActionResult Index(string q, string categoria, bool? stockBajo)
        {
            ViewBag.Busqueda = q;
            ViewBag.Categoria = categoria;
            ViewBag.StockBajo = stockBajo == true;
            ViewBag.UmbralStockBajo = UmbralStockBajo;
            ViewBag.Categorias = new SelectList(Uow.Inventario.GetCategorias(), categoria);

            int? maximo = stockBajo == true ? (int?)UmbralStockBajo : null;
            return View(Uow.Inventario.Buscar(q, categoria, maximo));
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return IdInvalido();
            var producto = Uow.Inventario.GetById(id.Value);
            if (producto == null) return HttpNotFound();
            return View(producto);
        }

        public ActionResult Create()
        {
            CargarCategoriasSugeridas();
            return View(new Inventario { stock = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = CamposEditables)] Inventario producto)
        {
            Normalizar(producto);

            if (!ModelState.IsValid)
            {
                CargarCategoriasSugeridas();
                return View(producto);
            }

            Uow.Inventario.Add(producto);
            Uow.Complete();

            Exito("Producto agregado al inventario.");
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return IdInvalido();
            var producto = Uow.Inventario.GetById(id.Value);
            if (producto == null) return HttpNotFound();

            CargarCategoriasSugeridas();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id," + CamposEditables)] Inventario producto)
        {
            Normalizar(producto);

            if (!ModelState.IsValid)
            {
                CargarCategoriasSugeridas();
                return View(producto);
            }

            Uow.Inventario.Update(producto);
            Uow.Complete();

            Exito("Producto actualizado.");
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var producto = Uow.Inventario.GetById(id.Value);
            if (producto == null) return HttpNotFound();

            ViewBag.EnUso = Uow.Inventario.TieneMovimientos(id.Value);
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Uow.Inventario.TieneMovimientos(id))
            {
                MensajeError("No se puede eliminar: el producto ya se usó en tickets. " +
                             "Puede dejar su stock en 0 en su lugar.");
                return RedirectToAction("Index");
            }

            Uow.Inventario.RemoveById(id);
            Uow.Complete();

            Exito("Producto eliminado.");
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Ajuste rápido de stock desde la lista: delta positivo = ingreso de mercadería,
        /// negativo = salida/merma. No permite dejar el stock en negativo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AjustarStock(int id, int delta)
        {
            if (delta == 0)
            {
                MensajeError("Indique una cantidad distinta de cero.");
                return RedirectToAction("Index");
            }

            if (Uow.Inventario.GetById(id) == null) return HttpNotFound();

            if (delta > 0)
            {
                Uow.Inventario.ReponerStock(id, delta);
            }
            else if (!Uow.Inventario.DescontarStock(id, -delta))
            {
                MensajeError("Stock insuficiente. Disponible: " + Uow.Inventario.ObtenerStock(id) + ".");
                return RedirectToAction("Index");
            }

            Exito("Stock ajustado. Nuevo stock: " + Uow.Inventario.ObtenerStock(id) + ".");
            return RedirectToAction("Index");
        }

        /// <summary>
        /// JSON con los productos con stock que coinciden con las specs de un vehículo.
        /// Útil para llenar selectores por AJAX: GET /Inventario/Compatibles?idVehiculo=3
        /// </summary>
        public ActionResult Compatibles(int? idVehiculo)
        {
            if (idVehiculo == null) return IdInvalido();
            var vehiculo = Uow.Vehiculos.GetConDetalle(idVehiculo.Value);
            if (vehiculo == null) return HttpNotFound();

            var datos = Uow.Inventario.GetCompatibles(vehiculo.VehiculoSpecs)
                .Select(i => new
                {
                    i.id,
                    i.nombre_producto,
                    i.categoria,
                    i.especificacion_tecnica,
                    stock = i.stock ?? 0,
                    i.precio
                })
                .ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        // ---- helpers ----

        private static void Normalizar(Inventario p)
        {
            if (p.nombre_producto != null) p.nombre_producto = p.nombre_producto.Trim();
            if (p.categoria != null) p.categoria = p.categoria.Trim();
            if (p.stock == null) p.stock = 0;
        }

        /// <summary>Categorías conocidas + las ya usadas, para un &lt;datalist&gt; en el formulario.</summary>
        private void CargarCategoriasSugeridas()
        {
            ViewBag.CategoriasSugeridas = CategoriaInventario.Todas
                .Union(Uow.Inventario.GetCategorias())
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}
