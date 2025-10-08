# 🍔 TechFood Solutions

![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)

Sistema de delivery de comida desarrollado con ASP.NET Core MVC, similar a Uber Eats y Pedidos Ya.

## 📋 Descripción

TechFood Solutions es una aplicación web que conecta a clientes con restaurantes para realizar pedidos de comida a domicilio. La plataforma permite a los usuarios explorar menús, realizar pedidos, y a los restaurantes gestionar sus productos y pedidos en tiempo real.

## 🚀 Características Principales

- **Gestión de Usuarios**: Sistema de autenticación con tres roles (Cliente, Asociado, Administrador)
- **Catálogo de Restaurantes**: Exploración y búsqueda de restaurantes disponibles
- **Carrito de Compras**: Gestión de productos antes de confirmar el pedido
- **Seguimiento de Pedidos**: Actualización en tiempo real del estado de los pedidos
- **Panel de Administración**: Control completo de restaurantes, usuarios y estadísticas
- **Panel de Asociado**: Gestión de menú, pedidos y reportes de ventas
- **API REST**: Endpoints documentados con Swagger para integración externa

## 🛠️ Stack Tecnológico

- **Framework**: ASP.NET Core MVC
- **Lenguaje**: C#
- **Base de Datos**: SQL Server
- **ORM**: Entity Framework Core
- **Arquitectura**: MVC (Model-View-Controller)
- **Documentación API**: Swagger/OpenAPI
- **Frontend**: Razor Pages, HTML, CSS, JavaScript

## 📁 Estructura del Proyecto

```
TechFood-Solutions/
├── Controllers/          # Controladores MVC
├── Models/              # Entidades del dominio
├── Views/               # Vistas Razor organizadas por área
│   ├── Account/         # Autenticación y registro
│   ├── Admin/           # Panel de administración
│   ├── Asociado/        # Panel del restaurante
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
- Realizar y rastrear pedidos
- Ver historial de compras

### 🏪 Asociado (Dueño de Restaurante)
- Gestionar menú de productos
- Recibir y procesar pedidos
- Actualizar estado de pedidos
- Ver reportes de ventas

### 👨‍💼 Administrador
- Agregar y gestionar restaurantes
- Administrar usuarios del sistema
- Asignar asociados a restaurantes
- Acceso a estadísticas globales

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
- Swagger: `https://localhost:5001/swagger`

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

## 📚 API Endpoints

La API REST está documentada con Swagger. Principales endpoints:

- `GET /api/restaurantes` - Obtener lista de restaurantes
- `GET /api/restaurantes/{id}/productos` - Obtener menú de un restaurante
- `POST /api/pedidos` - Crear nuevo pedido
- `PUT /api/pedidos/{id}/estado` - Actualizar estado de pedido
- `POST /api/usuarios/login` - Autenticar usuario
- `POST /api/usuarios/register` - Registrar nuevo usuario

Documentación completa disponible en `/swagger` al ejecutar la aplicación.

## 🗃️ Modelo de Datos Principal

- **Usuario**: Información base de todos los usuarios
- **Restaurante**: Datos de los establecimientos
- **Producto**: Items del menú de cada restaurante
- **Pedido**: Órdenes realizadas por clientes
- **DetallePedido**: Items individuales de cada pedido
- **Categoria**: Clasificación de productos

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

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📞 Contacto

- **Proyecto**: TechFood Solutions
- **Email**: soporte@techfood.com
- **Website**: https://techfood-solutions.com

## 🙏 Agradecimientos

- ASP.NET Core Team
- Entity Framework Team
- Comunidad de desarrolladores .NET

---

⭐ Si este proyecto te ha sido útil, considera darle una estrella en GitHub

**Desarrollado con ❤️ usando .NET Core**
