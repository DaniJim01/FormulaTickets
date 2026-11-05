-- ==========================================
-- CREACIÓN DE LA BASE DE DATOS
-- ==========================================
CREATE DATABASE FormulaTicketsDB;
GO

USE FormulaTicketsDB;
GO
-- ==========================================
-- 1. GESTIÓN DE CLIENTES
-- ==========================================

CREATE TABLE Cliente (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    telefono_1 VARCHAR(20),
    telefono_2 VARCHAR(20)
);

CREATE TABLE Direccion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente INT NOT NULL,
    provincia VARCHAR(50),
    ciudad VARCHAR(50),
    zip VARCHAR(20),
    calle TEXT,
    CONSTRAINT fk_direccion_cliente FOREIGN KEY (id_cliente) REFERENCES Cliente(id) ON DELETE CASCADE
);

-- ==========================================
-- 2. CATÁLOGO Y VEHÍCULOS
-- ==========================================

CREATE TABLE VehiculoSpecs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    anio INT NOT NULL,
    viscosidad_aceite_requerida VARCHAR(50),
    medida_llanta_requerida VARCHAR(50),
    tipo_filtro_requerido VARCHAR(50),
    modelo_caliper_requerido VARCHAR(50),
    modelo_pastilla_requerido VARCHAR(50),
    medida_aro_requerida VARCHAR(50)
);

CREATE TABLE Vehiculo (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente INT NOT NULL,
    id_vehiculospecs INT NOT NULL,
    placa VARCHAR(20) UNIQUE NOT NULL,
    kilometraje INT DEFAULT 0,
    CONSTRAINT fk_vehiculo_cliente FOREIGN KEY (id_cliente) REFERENCES Cliente(id) ON DELETE CASCADE,
    CONSTRAINT fk_vehiculo_specs FOREIGN KEY (id_vehiculospecs) REFERENCES VehiculoSpecs(id)
);

-- ==========================================
-- 3. INVENTARIO CENTRALIZADO
-- ==========================================

CREATE TABLE Inventario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre_producto VARCHAR(100) NOT NULL,
    categoria VARCHAR(50) NOT NULL, -- Ej: 'Aceite', 'Filtro', 'Llanta'
    especificacion_tecnica VARCHAR(50), -- Ej: '5W-30', '205/55R16'
    stock INT DEFAULT 0,
    precio DECIMAL(10, 2) NOT NULL
);

-- ==========================================
-- 4. OPERACIÓN DEL TALLER (TICKETS)
-- ==========================================

CREATE TABLE Ticket (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_vehiculo INT NOT NULL,
    estado VARCHAR(50) NOT NULL, -- Ej: 'En revisión', 'Esperando repuestos', 'Terminado'
    fecha_creacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_ticket_vehiculo FOREIGN KEY (id_vehiculo) REFERENCES Vehiculo(id) ON DELETE CASCADE
);

CREATE TABLE Updates (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ticket INT NOT NULL,
    fecha DATETIME DEFAULT GETDATE(),
    titulo VARCHAR(100) NOT NULL,
    descripcion TEXT NOT NULL,
    CONSTRAINT fk_updates_ticket FOREIGN KEY (id_ticket) REFERENCES Ticket(id) ON DELETE CASCADE
);

CREATE TABLE Detalle_Ticket (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ticket INT NOT NULL,
    id_inventario INT NOT NULL,
    cantidad INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_detalle_ticket FOREIGN KEY (id_ticket) REFERENCES Ticket(id) ON DELETE CASCADE,
    CONSTRAINT fk_detalle_inventario FOREIGN KEY (id_inventario) REFERENCES Inventario(id)
);
