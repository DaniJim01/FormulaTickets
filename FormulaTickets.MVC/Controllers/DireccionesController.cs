using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    /// <summary>
    /// Direcciones de un cliente. Siempre se llega desde Clientes/Details/{id}
    /// y se vuelve a esa pantalla al terminar.
    /// </summary>
    public class DireccionesController : BaseController
    {
        public DireccionesController() { }
        public DireccionesController(IUnitOfWork uow) : base(uow) { }

        // GET: Direcciones/Create?idCliente=5
        public ActionResult Create(int? idCliente)
        {
            if (idCliente == null) return IdInvalido();
            var cliente = Uow.Clientes.GetById(idCliente.Value);
            if (cliente == null) return HttpNotFound();

            ViewBag.Cliente = cliente;
            return View(new Direccion { id_cliente = cliente.id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_cliente,provincia,ciudad,zip,calle")] Direccion direccion)
        {
            var cliente = Uow.Clientes.GetById(direccion.id_cliente);
            if (cliente == null) return HttpNotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Cliente = cliente;
                return View(direccion);
            }

            Uow.Direcciones.Add(direccion);
            Uow.Complete();

            Exito("Dirección agregada.");
            return RedirectToAction("Details", "Clientes", new { id = direccion.id_cliente });
        }

        // GET: Direcciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return IdInvalido();
            var direccion = Uow.Direcciones.GetById(id.Value);
            if (direccion == null) return HttpNotFound();

            ViewBag.Cliente = Uow.Clientes.GetById(direccion.id_cliente);
            return View(direccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,id_cliente,provincia,ciudad,zip,calle")] Direccion direccion)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cliente = Uow.Clientes.GetById(direccion.id_cliente);
                return View(direccion);
            }

            Uow.Direcciones.Update(direccion);
            Uow.Complete();

            Exito("Dirección actualizada.");
            return RedirectToAction("Details", "Clientes", new { id = direccion.id_cliente });
        }

        // GET: Direcciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var direccion = Uow.Direcciones.GetById(id.Value);
            if (direccion == null) return HttpNotFound();

            ViewBag.Cliente = Uow.Clientes.GetById(direccion.id_cliente);
            return View(direccion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var direccion = Uow.Direcciones.GetById(id);
            if (direccion == null) return HttpNotFound();

            int idCliente = direccion.id_cliente;
            Uow.Direcciones.Remove(direccion);
            Uow.Complete();

            Exito("Dirección eliminada.");
            return RedirectToAction("Details", "Clientes", new { id = idCliente });
        }
    }
}
