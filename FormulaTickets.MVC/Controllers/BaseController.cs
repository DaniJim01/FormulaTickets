using System.Linq;
using System.Net;
using System.Web.Mvc;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.MVC.Controllers
{
    /// <summary>
    /// Base de todos los controllers: crea el UnitOfWork (un DbContext por petición),
    /// lo libera al terminar y ofrece mensajes flash para las vistas
    /// (TempData["Exito"], TempData["Advertencia"], TempData["Error"]).
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly IUnitOfWork Uow;

        protected BaseController() : this(new UnitOfWork()) { }

        protected BaseController(IUnitOfWork uow)
        {
            Uow = uow;
        }

        protected void Exito(string mensaje) { TempData["Exito"] = mensaje; }
        protected void Advertencia(string mensaje) { TempData["Advertencia"] = mensaje; }
        protected void MensajeError(string mensaje) { TempData["Error"] = mensaje; }

        protected ActionResult IdInvalido()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>Concatena los errores de validación del ModelState en un solo texto.</summary>
        protected string ErroresDeModelo()
        {
            return string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m)));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Uow != null) Uow.Dispose();
            base.Dispose(disposing);
        }
    }
}
