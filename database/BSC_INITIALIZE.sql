CREATE DATABASE BSC;
GO

USE BSC;
GO

CREATE TABLE dbo.Rol (
	RolID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Nombre VARCHAR(40) UNIQUE NOT NULL
);

CREATE TABLE dbo.Usuario (
	UsuarioID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	NombreUsuario VARCHAR(100) UNIQUE NOT NULL,
	ContrasenaHash VARCHAR(512) NOT NULL,
	RolID INT NOT NULL,
	CONSTRAINT FK_Usuario_Rol
		FOREIGN KEY (RolID) REFERENCES dbo.Rol(RolID)
);

CREATE TABLE dbo.Producto (
	ProductoID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Nombre VARCHAR(100) NOT NULL,
	Inventario INT NOT NULL,
	ClaveProducto VARCHAR(50) UNIQUE NOT NULL,

	CONSTRAINT CK_Producto_Inventario CHECK (Inventario >= 0)	
);

CREATE TABLE dbo.Pedido (
	PedidoID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	VendedorID INT NOT NULL,
	Cliente VARCHAR(100) NOT NULL,
	FechaPedido DATETIME NOT NULL,
	CONSTRAINT FK_Pedido_Usuario
		FOREIGN KEY (VendedorID) REFERENCES dbo.Usuario(UsuarioID)
);

CREATE TABLE dbo.DetallePedido (
	ProductoID INT NOT NULL,
	PedidoID INT NOT NULL,
	Cantidad INT NOT NULL,
	
	CONSTRAINT FK_DetallePedido_Producto
		FOREIGN KEY (ProductoID) REFERENCES dbo.Producto(ProductoID),
	CONSTRAINT FK_DetallePedido_Pedido
		FOREIGN KEY (PedidoID) REFERENCES dbo.Pedido(PedidoID),

	CONSTRAINT PK_DetallePedido
		PRIMARY kEY(PedidoID, ProductoID),

	CONSTRAINT CK_DetallePedido_Cantidad CHECK (Cantidad > 0)
);
GO

CREATE PROCEDURE dbo.RegistrarProducto
	@Nombre VARCHAR(100),
	@Inventario INT,
	@ClaveProducto VARCHAR(50)
AS 
BEGIN
	INSERT INTO dbo.Producto (Nombre, Inventario, ClaveProducto)
	VALUES (@Nombre, @Inventario, @ClaveProducto);
END;
GO

CREATE PROCEDURE dbo.ConsultarInventario AS 
BEGIN
	SELECT 
		ProductoID,
		Nombre, 
		ClaveProducto, 
		Inventario 
	FROM dbo.Producto
	ORDER BY Nombre;
END;
GO

CREATE PROCEDURE dbo.RegistrarInventario
	@ProductoID INT,
	@Cantidad INT
AS 
BEGIN
	IF @Cantidad IS NULL OR @Cantidad <=0
	BEGIN
		THROW 50001, 'La cantidad debe ser mayor a cero',1;
	END;
	
	UPDATE dbo.Producto
	SET Inventario = Inventario + @Cantidad 
	WHERE ProductoID = @ProductoID;

	IF @@ROWCOUNT = 0
	BEGIN
		THROW 50005, 'El producto no existe.', 1;
	END

END;
GO


CREATE VIEW dbo.vw_PedidosDetalle
AS 
	SELECT
		P.PedidoID, 
		U.NombreUsuario AS Vendedor, 
		P.Cliente, 
		P.FechaPedido as Fecha_Pedido, 
		DP.ProductoID AS ProductoID, 
		DP.Cantidad as Cantidad, 
		Prod.Nombre as Nombre_Producto,
		Prod.ClaveProducto as Clave_Producto
	FROM dbo.Pedido P 
		JOIN dbo.DetallePedido DP 
			ON P.PedidoId = DP.PedidoID 
		JOIN dbo.Producto Prod 
			ON DP.ProductoID = Prod.ProductoID 
		JOIN dbo.Usuario U 
			ON P.VendedorID = U.UsuarioID;
GO

CREATE TYPE dbo.ProductosPedido AS TABLE 
(
	ProductoID INT NOT NULL PRIMARY KEY,
	Cantidad INT NOT NULL CHECK(Cantidad > 0)
);

GO

CREATE PROCEDURE dbo.ProcesarPedido
	@VendedorId INT,
	@Cliente VARCHAR(100),
	@Productos dbo.ProductosPedido READONLY
AS
BEGIN
	SET XACT_ABORT ON; --Aborta los cambios en caso de errores


	IF @Cliente IS NULL OR @Cliente = ''
	BEGIN 
		THROW 50001, 'El nombre del cliente es obligatorio', 1;
	END;

	IF NOT EXISTS(SELECT 1 FROM @Productos)
	BEGIN 
		THROW 50002, 'El pedido debe contener productos', 1;
	END;

	IF NOT EXISTS (
		SELECT 1 FROM dbo.Usuario U 
		JOIN dbo.Rol R on R.RolId = U.RolID
		WHERE U.UsuarioID = @VendedorID AND R.Nombre = 'Vendedor'
	)
	BEGIN 
		THROW 50003, 'El usuario no existe o no es vendedor', 1;
	END;

	DECLARE @ProductosEsperados INT;
	DECLARE @ProductosActualizados INT;
	DECLARE @PedidoID INT;

	SELECT @ProductosEsperados = COUNT(*)
	FROM @Productos;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE P
		Set P.Inventario = P.Inventario - D.Cantidad
		FROM dbo.Producto AS P
		JOIN @Productos D ON P.ProductoID = D.ProductoID
		WHERE P.Inventario >=D.Cantidad;

		SET @ProductosActualizados = @@ROWCOUNT;

		IF @ProductosActualizados <> @ProductosEsperados
		BEGIN 
			THROW 50004, 'Algun producto no existe o no tiene inventario suficiente',1;
		END;

		INSERT INTO dbo.Pedido (VendedorID, Cliente, FechaPedido)
		VALUES(@VendedorId, @Cliente, GETDATE())

		-- obtiene el valor de la ultima insercion 
		SET @PedidoID = CONVERT(INT, SCOPE_IDENTITY());

		-- inserta el id del ultimo pedido junto con los valores de la lista de productos
		INSERT INTO dbo.DetallePedido (PedidoID, ProductoID, Cantidad)
		SELECT @PedidoID, ProductoID, Cantidad
		FROM  @Productos;

		COMMIT TRANSACTION;

		SELECT @PedidoID AS PedidoID;
	END TRY
	BEGIN CATCH 
		IF XACT_STATE() <> 0
		BEGIN 
			ROLLBACK TRANSACTION;
		END;

		THROW;
	END CATCH;
END;

GO

-- indice para ordenar los productos por su nombre
CREATE INDEX IX_Producto_Nombre
ON dbo.Producto (Nombre)
INCLUDE (ClaveProducto, Inventario);

-- tabla de historial para utilizar un trigger para llevar un registro de los cambios en el inventario

CREATE TABLE dbo.HistorialInventario (
	HistorialID INT IDENTITY(1, 1) PRIMARY KEY,
	ProductoID INT NOT NULL,
	InventarioAnterior INT NOT NULL,
	InventarioNuevo INT NOT NULL,
	Fecha DATETIME NOT NULL
);

GO


CREATE TRIGGER dbo.tr_Producto_Inventario 
ON dbo.Producto
AFTER UPDATE
AS 
BEGIN
	INSERT INTO dbo.HistorialInventario (
		ProductoID,
		InventarioAnterior,
		InventarioNuevo,
		Fecha
	)
	SELECT
		I.ProductoID,
		D.Inventario,
		I.Inventario,
		GETDATE()
	FROM inserted I 
	JOIN deleted D
		ON I.ProductoID = D.ProductoID
	WHERE I.Inventario <> D.Inventario;
END;
GO 


-- INSERCIONES

INSERT INTO dbo.Rol (Nombre)
SELECT V.Nombre
FROM (VALUES
    ('Administrador'),
    ('Personal administrativo'),
    ('Vendedor')
) AS V(Nombre)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Rol AS R
    WHERE R.Nombre = V.Nombre
);

-- USUARIO ADMINISTRADOR [admin, administrador1]
INSERT INTO dbo.Usuario (NombreUsuario, ContrasenaHash, RolID)
VALUES (
    'admin',
    'AQAAAAIAAYagAAAAECL+RHi3yLegvK5JzoYb7/5HEapq+K902GG+lONuzvgl9MCjz3qMhmcr2DyCASfSHg==',
    1
);

INSERT INTO dbo.Producto (Nombre, Inventario, ClaveProducto)
SELECT V.Nombre, V.Inventario, V.ClaveProducto
FROM (VALUES
    ('Teclado', 10, 'TEC-001'),
    ('Mouse',   15, 'MOU-001'),
    ('Monitor',  8, 'MON-001')
) AS V(Nombre, Inventario, ClaveProducto)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Producto AS P
    WHERE P.ClaveProducto = V.ClaveProducto
);
GO

