using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    public class ClientesController : BaseController
    {
        public ClientesController() { }
        public ClientesController(IUnitOfWork uow) : base(uow) { }

        // GET: Clientes?q=juan
        public ActionResult Index(string q)
        {
            ViewBag.Busqueda = q;
            return View(Uow.Clientes.Buscar(q));
        }

        // GET: Clientes/Details/5  (incluye direcciones y vehículos)
        public ActionResult Details(int? id)
        {
            if (id == null) return IdInvalido();
            var cliente = Uow.Clientes.GetCompleto(id.Value);
            if (cliente == null) return HttpNotFound();
            return View(cliente);
        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "nombre,telefono_1,telefono_2")] Cliente cliente)
        {
            if (!ModelState.IsValid) return View(cliente);

            cliente.nombre = cliente.nombre.Trim();
            Uow.Clientes.Add(cliente);
            Uow.Complete();

            Exito("Cliente creado. Ahora puede agregarle una dirección y sus vehículos.");
            return RedirectToAction("Details", new { id = cliente.id });
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return IdInvalido();
            var cliente = Uow.Clientes.GetById(id.Value);
            if (cliente == null) return HttpNotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombre,telefono_1,telefono_2")] Cliente cliente)
        {
            if (!ModelState.IsValid) return View(cliente);

            cliente.nombre = cliente.nombre.Trim();
            Uow.Clientes.Update(cliente);
            Uow.Complete();

            Exito("Cliente actualizado.");
            return RedirectToAction("Details", new { id = cliente.id });
        }

        // GET: Clientes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var cliente = Uow.Clientes.GetCompleto(id.Value);
            if (cliente == null) return HttpNotFound();

            ViewBag.TotalTickets = Uow.Tickets.ContarPorCliente(id.Value);
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // La BD borra en cascada direcciones, vehículos y tickets. Para no perder
            // el historial del taller por accidente, se bloquea si hay tickets.
            int tickets = Uow.Tickets.ContarPorCliente(id);
            if (tickets > 0)
            {
                MensajeError("No se puede eliminar: el cliente tiene " + tickets +
                             " ticket(s) en su historial.");
                return RedirectToAction("Details", new { id });
            }

            try
            {
                Uow.Clientes.RemoveById(id);
                Uow.Complete();
            }
            catch (DbUpdateException)
            {
                MensajeError("No se pudo eliminar el cliente porque tiene registros relacionados.");
                return RedirectToAction("Details", new { id });
            }

            Exito("Cliente eliminado.");
            return RedirectToAction("Index");
        }
    }
}
