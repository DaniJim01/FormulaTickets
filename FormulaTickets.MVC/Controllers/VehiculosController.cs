using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    public class VehiculosController : BaseController
    {
        private const string CamposEditables = "id_cliente,id_vehiculospecs,placa,kilometraje";

        public VehiculosController() { }
        public VehiculosController(IUnitOfWork uow) : base(uow) { }

        // GET: Vehiculos?q=ABC123
        public ActionResult Index(string q)
        {
            ViewBag.Busqueda = q;
            return View(Uow.Vehiculos.Buscar(q));
        }

        // GET: Vehiculos/Details/5  (cliente, specs e historial de tickets)
        public ActionResult Details(int? id)
        {
            if (id == null) return IdInvalido();
            var vehiculo = Uow.Vehiculos.GetConDetalle(id.Value);
            if (vehiculo == null) return HttpNotFound();
            return View(vehiculo);
        }

        // GET: Vehiculos/Create?idCliente=5
        public ActionResult Create(int? idCliente)
        {
            var vehiculo = new Vehiculo { id_cliente = idCliente ?? 0, kilometraje = 0 };
            CargarListas(vehiculo);
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = CamposEditables)] Vehiculo vehiculo)
        {
            Normalizar(vehiculo);

            if (ModelState.IsValid && Uow.Vehiculos.PlacaExiste(vehiculo.placa))
                ModelState.AddModelError("placa", "Ya existe un vehículo registrado con esa placa.");

            if (!ModelState.IsValid)
            {
                CargarListas(vehiculo);
                return View(vehiculo);
            }

            Uow.Vehiculos.Add(vehiculo);
            Uow.Complete();

            Exito("Vehículo registrado.");
            return RedirectToAction("Details", new { id = vehiculo.id });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return IdInvalido();
            var vehiculo = Uow.Vehiculos.GetById(id.Value);
            if (vehiculo == null) return HttpNotFound();

            CargarListas(vehiculo);
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id," + CamposEditables)] Vehiculo vehiculo)
        {
            Normalizar(vehiculo);

            if (ModelState.IsValid && Uow.Vehiculos.PlacaExiste(vehiculo.placa, vehiculo.id))
                ModelState.AddModelError("placa", "Ya existe otro vehículo registrado con esa placa.");

            if (!ModelState.IsValid)
            {
                CargarListas(vehiculo);
                return View(vehiculo);
            }

            // Aviso (no bloquea): el odómetro normalmente no baja.
            var actual = Uow.Vehiculos.Find(v => v.id == vehiculo.id).FirstOrDefault(); // sin tracking
            if (actual != null && (vehiculo.kilometraje ?? 0) < (actual.kilometraje ?? 0))
                Advertencia("El kilometraje nuevo (" + vehiculo.kilometraje + ") es menor al anterior (" +
                            actual.kilometraje + "). Verifique que sea correcto.");

            Uow.Vehiculos.Update(vehiculo);
            Uow.Complete();

            Exito("Vehículo actualizado.");
            return RedirectToAction("Details", new { id = vehiculo.id });
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var vehiculo = Uow.Vehiculos.GetConDetalle(id.Value);
            if (vehiculo == null) return HttpNotFound();
            return View(vehiculo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            int tickets = Uow.Tickets.ContarPorVehiculo(id);
            if (tickets > 0)
            {
                MensajeError("No se puede eliminar: el vehículo tiene " + tickets +
                             " ticket(s) en su historial.");
                return RedirectToAction("Details", new { id });
            }

            var vehiculo = Uow.Vehiculos.GetById(id);
            if (vehiculo == null) return HttpNotFound();
            int idCliente = vehiculo.id_cliente;

            try
            {
                Uow.Vehiculos.Remove(vehiculo);
                Uow.Complete();
            }
            catch (DbUpdateException)
            {
                MensajeError("No se pudo eliminar el vehículo porque tiene registros relacionados.");
                return RedirectToAction("Details", new { id });
            }

            Exito("Vehículo eliminado.");
            return RedirectToAction("Details", "Clientes", new { id = idCliente });
        }

        // ---- helpers ----

        private static void Normalizar(Vehiculo v)
        {
            if (v.placa != null) v.placa = v.placa.Trim().ToUpperInvariant();
            if (v.kilometraje == null) v.kilometraje = 0;
        }

        private void CargarListas(Vehiculo v)
        {
            ViewBag.id_cliente = new SelectList(
                Uow.Clientes.GetAll().OrderBy(c => c.nombre), "id", "nombre", v.id_cliente);

            ViewBag.id_vehiculospecs = new SelectList(
                Uow.VehiculosSpecs.GetAll()
                    .OrderBy(s => s.marca).ThenBy(s => s.modelo).ThenByDescending(s => s.anio)
                    .Select(s => new { s.id, Texto = s.marca + " " + s.modelo + " " + s.anio }),
                "id", "Texto", v.id_vehiculospecs);
        }
    }
}
