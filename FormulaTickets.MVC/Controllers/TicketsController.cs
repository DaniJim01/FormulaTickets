using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FormulaTickets.DataAccess;
using FormulaTickets.DataAccess.Repositories;
using FormulaTickets.DataAccess.Services;
using FormulaTickets.MVC.Models;

namespace FormulaTickets.MVC.Controllers
{
    /// <summary>
    /// Pantalla principal del taller. Las acciones que modifican el ticket
    /// (estado, bitácora, repuestos) pasan por <see cref="ITicketService"/>,
    /// que se encarga de las transacciones y del stock.
    /// </summary>
    public class TicketsController : BaseController
    {
        private readonly ITicketService _service;

        public TicketsController()
        {
            _service = new TicketService(Uow);
        }

        public TicketsController(IUnitOfWork uow, ITicketService service) : base(uow)
        {
            _service = service;
        }

        // GET: Tickets?estado=En revisión&q=ABC123
        public ActionResult Index(string estado, string q)
        {
            ViewBag.EstadoActual = estado;
            ViewBag.Busqueda = q;
            ViewBag.Estados = new SelectList(EstadoTicket.Todos, estado);
            ViewBag.Conteos = Uow.Tickets.ContarPorEstado();

            return View(Uow.Tickets.Buscar(estado, q));
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return IdInvalido();
            var vm = ConstruirDetalle(id.Value);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        // GET: Tickets/Create?idVehiculo=3
        public ActionResult Create(int? idVehiculo)
        {
            var vm = new TicketCreateViewModel { id_vehiculo = idVehiculo ?? 0 };
            CargarVehiculos(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TicketCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var resultado = _service.CrearTicket(vm.id_vehiculo, vm.descripcion);
                if (resultado.Ok)
                {
                    Exito(resultado.Mensaje);
                    return RedirectToAction("Details", new { id = resultado.Id });
                }
                ModelState.AddModelError("", resultado.Mensaje);
            }

            CargarVehiculos(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(CambiarEstadoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MensajeError(ErroresDeModelo());
                return RedirectToAction("Details", new { id = vm.id_ticket });
            }

            Notificar(_service.CambiarEstado(vm.id_ticket, vm.nuevoEstado, vm.comentario));
            return RedirectToAction("Details", new { id = vm.id_ticket });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarActualizacion(NuevaActualizacionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MensajeError(ErroresDeModelo());
                return RedirectToAction("Details", new { id = vm.id_ticket });
            }

            Notificar(_service.AgregarActualizacion(vm.id_ticket, vm.titulo, vm.descripcion));
            return RedirectToAction("Details", new { id = vm.id_ticket });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarRepuesto(AgregarRepuestoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MensajeError(ErroresDeModelo());
                return RedirectToAction("Details", new { id = vm.id_ticket });
            }

            Notificar(_service.AgregarRepuesto(vm.id_ticket, vm.id_inventario, vm.cantidad));
            return RedirectToAction("Details", new { id = vm.id_ticket });
        }

        // idDetalle = Detalle_Ticket.id ; id_ticket solo sirve para volver a la pantalla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarRepuesto(int idDetalle, int id_ticket)
        {
            Notificar(_service.QuitarRepuesto(idDetalle));
            return RedirectToAction("Details", new { id = id_ticket });
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return IdInvalido();
            var ticket = Uow.Tickets.GetDetalle(id.Value);
            if (ticket == null) return HttpNotFound();
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var resultado = _service.EliminarTicket(id);
            if (!resultado.Ok)
            {
                MensajeError(resultado.Mensaje);
                return RedirectToAction("Details", new { id });
            }

            Exito(resultado.Mensaje);
            return RedirectToAction("Index");
        }

        // ---- helpers ----

        /// <summary>Traduce un ResultadoOperacion a los mensajes flash de la vista.</summary>
        private void Notificar(ResultadoOperacion r)
        {
            if (r.Ok)
            {
                Exito(r.Mensaje);
                if (!string.IsNullOrEmpty(r.Advertencia)) Advertencia(r.Advertencia);
            }
            else
            {
                MensajeError(r.Mensaje);
            }
        }

        private TicketDetailsViewModel ConstruirDetalle(int id)
        {
            var ticket = Uow.Tickets.GetDetalle(id);
            if (ticket == null) return null;

            var conStock = Uow.Inventario.Find(i => (i.stock ?? 0) > 0)
                .OrderBy(i => i.categoria).ThenBy(i => i.nombre_producto)
                .Select(i => new SelectListItem
                {
                    Value = i.id.ToString(),
                    Text = i.nombre_producto
                           + (string.IsNullOrWhiteSpace(i.especificacion_tecnica) ? "" : " (" + i.especificacion_tecnica + ")")
                           + " — " + i.categoria + " — stock: " + i.stock
                })
                .ToList();

            return new TicketDetailsViewModel
            {
                Ticket = ticket,
                Updates = ticket.Updates.OrderByDescending(u => u.fecha).ThenByDescending(u => u.id).ToList(),
                Repuestos = ticket.Detalle_Ticket.OrderBy(d => d.id).ToList(),
                Compatibles = Uow.Inventario.GetCompatibles(ticket.Vehiculo.VehiculoSpecs).ToList(),
                Productos = conStock,
                Estados = new SelectList(EstadoTicket.Todos, ticket.estado),
                CambioEstado = new CambiarEstadoViewModel { id_ticket = id, nuevoEstado = ticket.estado },
                NuevaActualizacion = new NuevaActualizacionViewModel { id_ticket = id },
                NuevoRepuesto = new AgregarRepuestoViewModel { id_ticket = id, cantidad = 1 }
            };
        }

        private void CargarVehiculos(TicketCreateViewModel vm)
        {
            vm.Vehiculos = Uow.Vehiculos.Buscar(null)
                .Select(v => new SelectListItem
                {
                    Value = v.id.ToString(),
                    Text = v.placa + " — " + v.VehiculoSpecs.marca + " " + v.VehiculoSpecs.modelo + " " +
                           v.VehiculoSpecs.anio + " (" + v.Cliente.nombre + ")",
                    Selected = v.id == vm.id_vehiculo
                })
                .ToList();
        }
    }
}
