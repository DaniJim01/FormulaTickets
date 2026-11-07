using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FormulaTickets.DataAccess;

namespace FormulaTickets.MVC.Models
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione un vehículo.")]
        [Display(Name = "Vehículo")]
        public int id_vehiculo { get; set; }

        [StringLength(2000, ErrorMessage = "Máximo 2000 caracteres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Motivo de ingreso / problema reportado")]
        public string descripcion { get; set; }

        public IEnumerable<SelectListItem> Vehiculos { get; set; }
    }

    public class CambiarEstadoViewModel
    {
        public int id_ticket { get; set; }

        [Required(ErrorMessage = "Seleccione el nuevo estado.")]
        [Display(Name = "Nuevo estado")]
        public string nuevoEstado { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Comentario (opcional)")]
        public string comentario { get; set; }
    }

    public class NuevaActualizacionViewModel
    {
        public int id_ticket { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Título")]
        public string titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }
    }

    public class AgregarRepuestoViewModel
    {
        public int id_ticket { get; set; }

        [Required(ErrorMessage = "Seleccione un producto.")]
        [Display(Name = "Producto")]
        public int id_inventario { get; set; }

        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000.")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }
    }

    /// <summary>Todo lo que necesita la pantalla de detalle del ticket.</summary>
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; }

        /// <summary>Bitácora, la más reciente primero.</summary>
        public IList<Updates> Updates { get; set; }

        public IList<Detalle_Ticket> Repuestos { get; set; }

        /// <summary>Productos con stock que coinciden con las specs del vehículo.</summary>
        public IList<Inventario> Compatibles { get; set; }

        /// <summary>Todos los productos con stock, para el selector de "agregar repuesto".</summary>
        public IEnumerable<SelectListItem> Productos { get; set; }

        public IEnumerable<SelectListItem> Estados { get; set; }

        public CambiarEstadoViewModel CambioEstado { get; set; }
        public NuevaActualizacionViewModel NuevaActualizacion { get; set; }
        public AgregarRepuestoViewModel NuevoRepuesto { get; set; }

        /// <summary>Total de repuestos (cantidad x precio actual del inventario).</summary>
        public decimal TotalRepuestos
        {
            get
            {
                return Repuestos == null
                    ? 0m
                    : Repuestos.Sum(d => d.cantidad * (d.Inventario == null ? 0m : d.Inventario.precio));
            }
        }

        public bool EstaTerminado
        {
            get { return Ticket != null && Ticket.estado == EstadoTicket.Terminado; }
        }
    }
}
