using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using TechFood_Solutions.Controllers;
using TechFood_Solutions.Models;
using TechFood_Solutions.Services;
using TechFood_Solutions.ViewModels;
using Xunit;

namespace TechFood_Solutions.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _mockCartService;
        private readonly CartController _controller;
        private readonly TechFoodDbContext _context;

        public CartControllerTests()
        {
            // Simulamos la base de datos en memoria
            var options = new DbContextOptionsBuilder<TechFoodDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new TechFoodDbContext(options);

            // Mock del servicio de carrito
            _mockCartService = new Mock<ICartService>();

            // Controlador a probar
            _controller = new CartController(_context, _mockCartService.Object);
        }


        /*      Pruebas para el método Index
                Devuelve una vista (ViewResult).

                El modelo es un Cart.

                El objeto es el mismo que el retornado por el mock.

                El total calculado es correcto (confirmando que la propiedad de solo lectura funciona).
         */
        [Fact]
        public void Index_ReturnsViewWithCart()
        {
            // Arrange
            var fakeCart = new Cart();
            fakeCart.Items.Add(new CartItem
            {
                MenuItemId = 1,
                Nombre = "Pizza Margarita",
                Cantidad = 2,
                Precio = 5.0m
            });

            _mockCartService.Setup(s => s.GetCart()).Returns(fakeCart);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.IsType<Cart>(result.Model);
            Assert.Equal(fakeCart, result.Model);
            Assert.Equal(10.0m, ((Cart)result.Model).Total); // 2 * 5.0
        }

        /*     
            Pruebas para el método UpdateQuantity
            HttpContext vacío (sin autenticación), para probar redirecciones por acceso no autorizado.
         */
        [Fact]
        public void UpdateQuantity_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.UpdateQuantity(1, 3) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        /*
            Simula un usuario autenticado usando ClaimsPrincipal.

            Llama al método con parámetros válidos.

            Verifica que:

            Se ejecuta _cartService.UpdateQuantity(...).

            La acción redirige correctamente a Index.

         */
        [Fact]
        public void UpdateQuantity_AuthenticatedUser_CallsServiceAndRedirectsToIndex()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = _controller.UpdateQuantity(2, 5) as RedirectToActionResult;

            // Assert
            _mockCartService.Verify(s => s.UpdateQuantity(2, 5), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }


        /*
            Pruebas para el método RemoveItem
            Usuario no autenticado debe ser redirigido a Login.
         */
        [Fact]
        public void RemoveItem_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.RemoveItem(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        /*
           Usuario autenticado con items en el carrito.
           Verifica que el JSON contiene:
               - hasItems = true
               - restaurantId correcto
               - restaurantName correcto
               - itemCount correcto
        */

        /*
            Pruebas para el método Checkout
            Usuario no autenticado debe ser redirigido a Login.
         */
        [Fact]
        public void Checkout_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Checkout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        /*
            Pruebas para el método MyOrders
            Usuario no autenticado debe ser redirigido a Login.
         */
        [Fact]
        public async Task MyOrders_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.MyOrders() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }
    }
}
