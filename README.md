# 🍔 TechFood Solutions

![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)

Sistema de delivery de comida desarrollado con ASP.NET Core MVC.

## 📋 Descripción

TechFood Solutions es una aplicación web que conecta a clientes con restaurantes para realizar pedidos de comida a domicilio. La plataforma permite a los usuarios explorar menús, realizar pedidos, y a los restaurantes gestionar sus productos y pedidos en tiempo real.

## 🚀 Características Principales

- **Gestión de Usuarios**: Sistema de autenticación con tres roles (Cliente, Asociado, Administrador)
- **Catálogo de Restaurantes**: Exploración y búsqueda de restaurantes disponibles
- **Carrito de Compras**: Gestión de productos antes de confirmar el pedido
- **Seguimiento de Pedidos**: Actualización en tiempo real del estado de los pedidos
- **Panel de Administración**: Control completo de asociados.
- **Panel de Asociado**: Gestión de menú y pedidos. 

## 🛠️ Stack Tecnológico

- **Framework**: ASP.NET Core MVC
- **Lenguaje**: C#
- **Base de Datos**: SQL Server
- **ORM**: Entity Framework Core
- **Arquitectura**: MVC (Model-View-Controller)
- **Frontend**: Razor Pages, HTML, CSS, JavaScript

## 📁 Estructura del Proyecto

```
TechFood-Solutions/
├── Controllers/          # Controladores MVC
├── Models/              # Entidades del dominio
├── Views/               # Vistas Razor organizadas por área
│   ├── Account/         # Autenticación y registro
│   ├── Admin/           # Panel de administración
│   ├── Asociado/        # Panel del asociado
│   ├── Cliente/         # Panel del cliente
│   ├── Cart/            # Carrito de compras
│   ├── Home/            # Página principal
│   └── Shared/          # Componentes compartidos
├── Services/            # Lógica de negocio
├── ViewModels/          # DTOs para las vistas
├── Migrations/          # Migraciones de Entity Framework
└── wwwroot/             # Archivos estáticos (CSS, JS, imágenes)
```

## 🎯 Roles de Usuario

### 👤 Cliente
- Explorar restaurantes y menús
- Agregar productos al carrito
- Realizar pedidos
- Ver historial de compras

### 🏪 Asociado (Dueño de Restaurante)
- Gestionar menú de productos
- Recibir y procesar pedidos
- Actualizar estado de pedidos
- Ver reportes de ventas

### 👨‍💼 Administrador
- Agregar y gestionar asociados
- Administrar asociados del sistema
- Asignar asociados a restaurantes

## ⚙️ Instalación y Configuración

### Requisitos Previos

- .NET 6.0 SDK o superior
- SQL Server 2019 o superior
- Visual Studio 2022 o VS Code

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/tu-usuario/techfood-solutions.git
cd techfood-solutions
```

2. **Configurar la cadena de conexión**

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TechFoodDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. **Ejecutar migraciones de base de datos**
```bash
dotnet ef database update
```

4. **Ejecutar la aplicación**
```bash
dotnet run
```

5. **Acceder a la aplicación**
- Aplicación: `https://localhost:5001`

## 🔧 Configuración Adicional

### Crear Migración
```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Revertir Migración
```bash
dotnet ef database update MigracionAnterior
```

### Eliminar Última Migración
```bash
dotnet ef migrations remove
```

## 🔐 Seguridad

- Autenticación mediante Cookie Authentication
- Contraseñas hasheadas
- Autorización basada en roles
- Protección CSRF en formularios
- Validación de datos en servidor

## 📈 Características Futuras

- [ ] Sistema de calificaciones y reseñas
- [ ] Integración con pasarelas de pago
- [ ] Notificaciones push en tiempo real
- [ ] Aplicación móvil (iOS/Android)
- [ ] Sistema de cupones y promociones
- [ ] Rastreo GPS del repartidor

## Manual Tecnico
[Manual Tecnico.pdf](https://github.com/user-attachments/files/22783241/Manual.Tecnico.pdf)

## Manual de Usuario
[MANUAL USUARIO.pdf](https://github.com/user-attachments/files/22783251/MANUAL.USUARIO.pdf)
