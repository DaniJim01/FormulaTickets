using System;
using FormulaTickets.DataAccess.Repositories;

namespace FormulaTickets.DataAccess.Services
{
    public interface ITicketService
    {
        ResultadoOperacion CrearTicket(int idVehiculo, string descripcionInicial);
        ResultadoOperacion CambiarEstado(int idTicket, string nuevoEstado, string comentario);
        ResultadoOperacion AgregarActualizacion(int idTicket, string titulo, string descripcion);
        ResultadoOperacion AgregarRepuesto(int idTicket, int idInventario, int cantidad);
        ResultadoOperacion QuitarRepuesto(int idDetalle);
        ResultadoOperacion EliminarTicket(int idTicket);
    }

    /// <summary>
    /// Reglas de negocio del ticket. Toda operación que toca stock se ejecuta
    /// dentro de una transacción y deja registro en la bitácora (tabla Updates).
    ///
    /// Regla de stock: el repuesto se DESCUENTA del inventario al agregarse al ticket
    /// y se DEVUELVE al quitarlo o al eliminar un ticket que no esté Terminado.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _uow;

        public TicketService(IUnitOfWork uow)
        {
            if (uow == null) throw new ArgumentNullException("uow");
            _uow = uow;
        }

        public ResultadoOperacion CrearTicket(int idVehiculo, string descripcionInicial)
        {
            if (_uow.Vehiculos.GetById(idVehiculo) == null)
                return ResultadoOperacion.Error("El vehículo seleccionado no existe.");

            var ahora = DateTime.Now;

            var ticket = new Ticket
            {
                id_vehiculo = idVehiculo,
                estado = EstadoTicket.EnRevision,
                fecha_creacion = ahora
            };

            _uow.Tickets.Add(ticket);
            _uow.Updates.Add(new Updates
            {
                Ticket = ticket,
                fecha = ahora,
                titulo = "Ticket creado",
                descripcion = string.IsNullOrWhiteSpace(descripcionInicial)
                    ? "El vehículo ingresó al taller."
                    : descripcionInicial.Trim()
            });

            _uow.Complete(); // una sola transacción: ticket + primera entrada de bitácora

            return ResultadoOperacion.Exito("Ticket creado correctamente.", null, ticket.id);
        }

        public ResultadoOperacion CambiarEstado(int idTicket, string nuevoEstado, string comentario)
        {
            if (!EstadoTicket.EsValido(nuevoEstado))
                return ResultadoOperacion.Error("El estado seleccionado no es válido.");

            var ticket = _uow.Tickets.GetById(idTicket);
            if (ticket == null)
                return ResultadoOperacion.Error("El ticket no existe.");

            if (ticket.estado == nuevoEstado)
                return ResultadoOperacion.Error("El ticket ya se encuentra en ese estado.");

            string anterior = ticket.estado;
            ticket.estado = nuevoEstado;

            string descripcion = anterior + " → " + nuevoEstado;
            if (!string.IsNullOrWhiteSpace(comentario))
                descripcion += Environment.NewLine + comentario.Trim();

            _uow.Updates.Add(new Updates
            {
                id_ticket = idTicket,
                fecha = DateTime.Now,
                titulo = "Cambio de estado",
                descripcion = descripcion
            });

            _uow.Complete();
            return ResultadoOperacion.Exito("Estado actualizado a \"" + nuevoEstado + "\".");
        }

        public ResultadoOperacion AgregarActualizacion(int idTicket, string titulo, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
                return ResultadoOperacion.Error("El título y la descripción son obligatorios.");

            if (titulo.Trim().Length > 100)
                return ResultadoOperacion.Error("El título no puede superar los 100 caracteres.");

            if (_uow.Tickets.GetById(idTicket) == null)
                return ResultadoOperacion.Error("El ticket no existe.");

            _uow.Updates.Add(new Updates
            {
                id_ticket = idTicket,
                fecha = DateTime.Now,
                titulo = titulo.Trim(),
                descripcion = descripcion.Trim()
            });

            _uow.Complete();
            return ResultadoOperacion.Exito("Actualización registrada.");
        }

        public ResultadoOperacion AgregarRepuesto(int idTicket, int idInventario, int cantidad)
        {
            if (cantidad < 1)
                return ResultadoOperacion.Error("La cantidad debe ser al menos 1.");

            var ticket = _uow.Tickets.GetById(idTicket);
            if (ticket == null)
                return ResultadoOperacion.Error("El ticket no existe.");

            if (ticket.estado == EstadoTicket.Terminado)
                return ResultadoOperacion.Error("El ticket está terminado. Cámbielo de estado para poder agregar repuestos.");

            var producto = _uow.Inventario.GetById(idInventario);
            if (producto == null)
                return ResultadoOperacion.Error("El producto no existe en el inventario.");

            // ¿Coincide con lo que el vehículo requiere? (aviso, no bloquea)
            string advertencia = null;
            var vehiculo = _uow.Vehiculos.GetConDetalle(ticket.id_vehiculo);
            string requerida = CategoriaInventario.RequeridaPor(producto.categoria, vehiculo == null ? null : vehiculo.VehiculoSpecs);
            if (!string.IsNullOrWhiteSpace(requerida)
                && !CategoriaInventario.Equivalentes(requerida, producto.especificacion_tecnica))
            {
                advertencia = string.Format(
                    "Atención: el vehículo requiere \"{0}\" para {1}, pero este producto es \"{2}\".",
                    requerida, producto.categoria,
                    string.IsNullOrWhiteSpace(producto.especificacion_tecnica) ? "sin especificación" : producto.especificacion_tecnica);
            }

            using (var tx = _uow.BeginTransaction())
            {
                // UPDATE atómico: falla (0 filas) si el stock no alcanza.
                if (!_uow.Inventario.DescontarStock(idInventario, cantidad))
                {
                    int disponible = _uow.Inventario.ObtenerStock(idInventario);
                    return ResultadoOperacion.Error(string.Format(
                        "Stock insuficiente de \"{0}\". Disponible: {1}.", producto.nombre_producto, disponible));
                }

                var linea = _uow.DetallesTicket.GetPorTicketEInventario(idTicket, idInventario);
                if (linea != null)
                {
                    linea.cantidad += cantidad;
                }
                else
                {
                    _uow.DetallesTicket.Add(new Detalle_Ticket
                    {
                        id_ticket = idTicket,
                        id_inventario = idInventario,
                        cantidad = cantidad
                    });
                }

                _uow.Updates.Add(new Updates
                {
                    id_ticket = idTicket,
                    fecha = DateTime.Now,
                    titulo = "Repuesto agregado",
                    descripcion = cantidad + " x " + producto.nombre_producto
                                  + (advertencia == null ? "" : Environment.NewLine + advertencia)
                });

                _uow.Complete();
                tx.Commit();
            }

            return ResultadoOperacion.Exito(
                cantidad + " x " + producto.nombre_producto + " agregado al ticket.", advertencia);
        }

        public ResultadoOperacion QuitarRepuesto(int idDetalle)
        {
            var linea = _uow.DetallesTicket.GetById(idDetalle);
            if (linea == null)
                return ResultadoOperacion.Error("El repuesto no existe en este ticket.");

            var ticket = _uow.Tickets.GetById(linea.id_ticket);
            if (ticket != null && ticket.estado == EstadoTicket.Terminado)
                return ResultadoOperacion.Error("El ticket está terminado. Cámbielo de estado para poder quitar repuestos.");

            var producto = _uow.Inventario.GetById(linea.id_inventario);
            string nombre = producto == null ? "Producto #" + linea.id_inventario : producto.nombre_producto;
            int cantidad = linea.cantidad;

            using (var tx = _uow.BeginTransaction())
            {
                _uow.Inventario.ReponerStock(linea.id_inventario, cantidad);

                _uow.Updates.Add(new Updates
                {
                    id_ticket = linea.id_ticket,
                    fecha = DateTime.Now,
                    titulo = "Repuesto retirado",
                    descripcion = cantidad + " x " + nombre + " (se devolvió al inventario)"
                });

                _uow.DetallesTicket.Remove(linea);

                _uow.Complete();
                tx.Commit();
            }

            return ResultadoOperacion.Exito(nombre + " retirado del ticket y devuelto al inventario.");
        }

        public ResultadoOperacion EliminarTicket(int idTicket)
        {
            var ticket = _uow.Tickets.GetById(idTicket);
            if (ticket == null)
                return ResultadoOperacion.Error("El ticket no existe.");

            using (var tx = _uow.BeginTransaction())
            {
                // Si el trabajo no terminó, los repuestos no se usaron: se devuelven al inventario.
                if (ticket.estado != EstadoTicket.Terminado)
                {
                    foreach (var linea in _uow.DetallesTicket.GetByTicket(idTicket))
                        _uow.Inventario.ReponerStock(linea.id_inventario, linea.cantidad);
                }

                // Updates y Detalle_Ticket se borran por ON DELETE CASCADE.
                _uow.Tickets.Remove(ticket);

                _uow.Complete();
                tx.Commit();
            }

            return ResultadoOperacion.Exito("Ticket eliminado.");
        }
    }
}
