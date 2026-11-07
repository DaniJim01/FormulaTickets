using System.ComponentModel.DataAnnotations;

// Validaciones y etiquetas para las entidades generadas por EF.
// Se usan clases "buddy" (MetadataType) para que NO se pierdan cuando
// se regenere el modelo desde Model1.edmx.
namespace FormulaTickets.DataAccess
{
    [MetadataType(typeof(ClienteMetadata))]
    public partial class Cliente { }

    public class ClienteMetadata
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "Solo números, espacios, + - ( )")]
        [Display(Name = "Teléfono principal")]
        public string telefono_1 { get; set; }

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "Solo números, espacios, + - ( )")]
        [Display(Name = "Teléfono secundario")]
        public string telefono_2 { get; set; }
    }

    [MetadataType(typeof(DireccionMetadata))]
    public partial class Direccion { }

    public class DireccionMetadata
    {
        [StringLength(50)]
        [Display(Name = "Provincia")]
        public string provincia { get; set; }

        [StringLength(50)]
        [Display(Name = "Ciudad")]
        public string ciudad { get; set; }

        [StringLength(20)]
        [Display(Name = "Código postal")]
        public string zip { get; set; }

        [Display(Name = "Dirección exacta")]
        public string calle { get; set; }
    }

    [MetadataType(typeof(VehiculoSpecsMetadata))]
    public partial class VehiculoSpecs { }

    public class VehiculoSpecsMetadata
    {
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(50)]
        [Display(Name = "Marca")]
        public string marca { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Modelo")]
        public string modelo { get; set; }

        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(1900, 2100, ErrorMessage = "Ingrese un año válido.")]
        [Display(Name = "Año")]
        public int anio { get; set; }

        [StringLength(50)]
        [Display(Name = "Viscosidad de aceite")]
        public string viscosidad_aceite_requerida { get; set; }

        [StringLength(50)]
        [Display(Name = "Medida de llanta")]
        public string medida_llanta_requerida { get; set; }

        [StringLength(50)]
        [Display(Name = "Tipo de filtro")]
        public string tipo_filtro_requerido { get; set; }

        [StringLength(50)]
        [Display(Name = "Modelo de caliper")]
        public string modelo_caliper_requerido { get; set; }

        [StringLength(50)]
        [Display(Name = "Modelo de pastilla")]
        public string modelo_pastilla_requerido { get; set; }

        [StringLength(50)]
        [Display(Name = "Medida de aro")]
        public string medida_aro_requerida { get; set; }
    }

    [MetadataType(typeof(VehiculoMetadata))]
    public partial class Vehiculo { }

    public class VehiculoMetadata
    {
        [Display(Name = "Cliente")]
        public int id_cliente { get; set; }

        [Display(Name = "Especificaciones del vehículo")]
        public int id_vehiculospecs { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria.")]
        [StringLength(20)]
        [Display(Name = "Placa")]
        public string placa { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje no puede ser negativo.")]
        [Display(Name = "Kilometraje")]
        public int? kilometraje { get; set; }
    }

    [MetadataType(typeof(InventarioMetadata))]
    public partial class Inventario { }

    public class InventarioMetadata
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Producto")]
        public string nombre_producto { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string categoria { get; set; }

        [StringLength(50)]
        [Display(Name = "Especificación técnica")]
        public string especificacion_tecnica { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int? stock { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Precio inválido.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio")]
        public decimal precio { get; set; }
    }

    [MetadataType(typeof(TicketMetadata))]
    public partial class Ticket { }

    public class TicketMetadata
    {
        [Display(Name = "Vehículo")]
        public int id_vehiculo { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Fecha de creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public System.DateTime? fecha_creacion { get; set; }
    }

    [MetadataType(typeof(UpdatesMetadata))]
    public partial class Updates { }

    public class UpdatesMetadata
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Fecha")]
        public System.DateTime? fecha { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Título")]
        public string titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }
    }

    [MetadataType(typeof(DetalleTicketMetadata))]
    public partial class Detalle_Ticket { }

    public class DetalleTicketMetadata
    {
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }
    }
}
