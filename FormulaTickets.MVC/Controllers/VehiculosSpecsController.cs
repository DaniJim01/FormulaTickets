using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    /// <summary>Catálogo de marcas/modelos/años y sus especificaciones requeridas.</summary>
    public class VehiculosSpecsController : BaseController
    {
        private const string CamposEditables =
            "marca,modelo,anio,viscosidad_aceite_requerida,medida_llanta_requerida,tipo_filtro_requerido," +
            "modelo_caliper_requerido,modelo_pastilla_requerido,medida_aro_requerida";

        public VehiculosSpecsController() { }
        public VehiculosSpecsController(IUnitOfWork uow) : base(uow) { }

        // GET: VehiculosSpecs?q=toyota&anio=2020
        public ActionResult Index(string q, int? anio)
        {
            ViewBag.Busqueda = q;
            ViewBag.Anio = anio;
            return View(Uow.VehiculosSpecs.Buscar(q, anio));
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return IdInvalido();
            var specs = Uow.VehiculosSpecs.GetById(id.Value);
            if (specs == null) return HttpNotFound();
            return View(specs);
        }

        public ActionResult Create()
        {
            return View(new VehiculoSpecs());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = CamposEditables)] VehiculoSpecs specs)
        {
            Normalizar(specs);

            if (ModelState.IsValid && Uow.VehiculosSpecs.Existe(specs.marca, specs.modelo, specs.anio))
                ModelState.AddModelError("", "Ya existe ese modelo para ese año en el catálogo.");

            if (!ModelState.IsValid) return View(specs);

            Uow.VehiculosSpecs.Add(specs);
            Uow.Complete();

            Exito("Especificaciones creadas.");
            return RedirectToAction("Details", new { id = specs.id });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return IdInvalido();
            var specs = Uow.VehiculosSpecs.GetById(id.Value);
            if (specs == null) return HttpNotFound();
            return View(specs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id," + CamposEditables)] VehiculoSpecs specs)
        {
            Normalizar(specs);

            if (ModelState.IsValid && Uow.VehiculosSpecs.Existe(specs.marca, specs.modelo, specs.anio, specs.id))
                ModelState.AddModelError("", "Ya existe ese modelo para ese año en el catálogo.");

            if (!ModelState.IsValid) return View(specs);

            Uow.VehiculosSpecs.Update(specs);
            Uow.Complete();

            Exito("Especificaciones actualizadas.");
            return RedirectToAction("Details", new { id = specs.id });
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var specs = Uow.VehiculosSpecs.GetById(id.Value);
            if (specs == null) return HttpNotFound();

            ViewBag.EnUso = Uow.VehiculosSpecs.TieneVehiculos(id.Value);
            return View(specs);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Uow.VehiculosSpecs.TieneVehiculos(id))
            {
                MensajeError("No se puede eliminar: hay vehículos registrados con estas especificaciones.");
                return RedirectToAction("Details", new { id });
            }

            try
            {
                Uow.VehiculosSpecs.RemoveById(id);
                Uow.Complete();
            }
            catch (DbUpdateException)
            {
                MensajeError("No se pudo eliminar el registro porque tiene datos relacionados.");
                return RedirectToAction("Details", new { id });
            }

            Exito("Especificaciones eliminadas.");
            return RedirectToAction("Index");
        }

        private static void Normalizar(VehiculoSpecs s)
        {
            if (s.marca != null) s.marca = s.marca.Trim();
            if (s.modelo != null) s.modelo = s.modelo.Trim();
        }
    }
}
