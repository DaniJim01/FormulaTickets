using System;
using System.Data.Entity;

namespace FormulaTickets.DataAccess.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        IDireccionRepository Direcciones { get; }
        IVehiculoSpecsRepository VehiculosSpecs { get; }
        IVehiculoRepository Vehiculos { get; }
        IInventarioRepository Inventario { get; }
        ITicketRepository Tickets { get; }
        IUpdatesRepository Updates { get; }
        IDetalleTicketRepository DetallesTicket { get; }

        /// <summary>Guarda todos los cambios pendientes (equivale a SaveChanges).</summary>
        int Complete();

        /// <summary>Inicia una transacción explícita (Commit / Dispose para rollback).</summary>
        DbContextTransaction BeginTransaction();
    }

    /// <summary>
    /// Comparte UN solo DbContext entre todos los repositories de una petición web.
    /// Se crea una instancia por controller y se descarta con él.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FormulaTicketsDBEntities _context;

        private IClienteRepository _clientes;
        private IDireccionRepository _direcciones;
        private IVehiculoSpecsRepository _vehiculosSpecs;
        private IVehiculoRepository _vehiculos;
        private IInventarioRepository _inventario;
        private ITicketRepository _tickets;
        private IUpdatesRepository _updates;
        private IDetalleTicketRepository _detallesTicket;

        public UnitOfWork() : this(new FormulaTicketsDBEntities()) { }

        public UnitOfWork(FormulaTicketsDBEntities context)
        {
            if (context == null) throw new ArgumentNullException("context");
            _context = context;
        }

        public IClienteRepository Clientes
        {
            get { return _clientes ?? (_clientes = new ClienteRepository(_context)); }
        }

        public IDireccionRepository Direcciones
        {
            get { return _direcciones ?? (_direcciones = new DireccionRepository(_context)); }
        }

        public IVehiculoSpecsRepository VehiculosSpecs
        {
            get { return _vehiculosSpecs ?? (_vehiculosSpecs = new VehiculoSpecsRepository(_context)); }
        }

        public IVehiculoRepository Vehiculos
        {
            get { return _vehiculos ?? (_vehiculos = new VehiculoRepository(_context)); }
        }

        public IInventarioRepository Inventario
        {
            get { return _inventario ?? (_inventario = new InventarioRepository(_context)); }
        }

        public ITicketRepository Tickets
        {
            get { return _tickets ?? (_tickets = new TicketRepository(_context)); }
        }

        public IUpdatesRepository Updates
        {
            get { return _updates ?? (_updates = new UpdatesRepository(_context)); }
        }

        public IDetalleTicketRepository DetallesTicket
        {
            get { return _detallesTicket ?? (_detallesTicket = new DetalleTicketRepository(_context)); }
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public DbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
