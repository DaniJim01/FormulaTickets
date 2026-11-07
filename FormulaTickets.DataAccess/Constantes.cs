using System;
using System.Linq;

namespace FormulaTickets.DataAccess
{
    /// <summary>
    /// Estados válidos de un ticket (columna Ticket.estado).
    /// Para agregar un estado nuevo basta con añadirlo aquí y en <see cref="Todos"/>.
    /// </summary>
    public static class EstadoTicket
    {
        public const string EnRevision = "En revisión";
        public const string EnReparacion = "En reparación";
        public const string EsperandoRepuestos = "Esperando repuestos";
        public const string Terminado = "Terminado";

        public static readonly string[] Todos =
        {
            EnRevision, EnReparacion, EsperandoRepuestos, Terminado
        };

        public static bool EsValido(string estado)
        {
            return Todos.Contains(estado);
        }
    }

    /// <summary>
    /// Categorías de Inventario que se pueden cruzar con VehiculoSpecs
    /// para saber si un producto es compatible con un vehículo.
    /// El valor de Inventario.categoria debe coincidir (sin importar mayúsculas).
    /// </summary>
    public static class CategoriaInventario
    {
        public const string Aceite = "Aceite";
        public const string Filtro = "Filtro";
        public const string Llanta = "Llanta";
        public const string Caliper = "Caliper";
        public const string Pastilla = "Pastilla";
        public const string Aro = "Aro";

        public static readonly string[] Todas =
        {
            Aceite, Filtro, Llanta, Caliper, Pastilla, Aro
        };

        /// <summary>
        /// Devuelve la especificación que el vehículo requiere para la categoría dada,
        /// o null si la categoría no depende de las specs del vehículo (ej. "Herramienta").
        /// </summary>
        public static string RequeridaPor(string categoria, VehiculoSpecs specs)
        {
            if (specs == null || string.IsNullOrWhiteSpace(categoria)) return null;

            switch (categoria.Trim().ToLowerInvariant())
            {
                case "aceite": return specs.viscosidad_aceite_requerida;
                case "filtro": return specs.tipo_filtro_requerido;
                case "llanta": return specs.medida_llanta_requerida;
                case "caliper": return specs.modelo_caliper_requerido;
                case "pastilla": return specs.modelo_pastilla_requerido;
                case "aro": return specs.medida_aro_requerida;
                default: return null;
            }
        }

        public static bool Equivalentes(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
