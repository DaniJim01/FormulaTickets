namespace FormulaTickets.DataAccess.Services
{
    /// <summary>
    /// Resultado de una operación de negocio. Evita usar excepciones para
    /// casos esperados (stock insuficiente, ticket terminado, etc.).
    /// </summary>
    public class ResultadoOperacion
    {
        public bool Ok { get; private set; }
        public string Mensaje { get; private set; }

        /// <summary>Aviso no bloqueante (ej. producto que no coincide con las specs del vehículo).</summary>
        public string Advertencia { get; private set; }

        /// <summary>Id del registro creado, cuando aplica.</summary>
        public int? Id { get; private set; }

        public static ResultadoOperacion Exito(string mensaje = null, string advertencia = null, int? id = null)
        {
            return new ResultadoOperacion { Ok = true, Mensaje = mensaje, Advertencia = advertencia, Id = id };
        }

        public static ResultadoOperacion Error(string mensaje)
        {
            return new ResultadoOperacion { Ok = false, Mensaje = mensaje };
        }
    }
}
