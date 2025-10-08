using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechFood_Solutions.Controllers;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Login_ReturnsRedirectToHome_WhenCredentialsAreValid()
        {
            // Arrange
            var userEmail = "test@example.com";
            var userPassword = "password123";
            var fakeUser = new User { UserName = "testuser", Email = userEmail };

            // Mock de UserManager
            var userStore = new Mock<IUserStore<User>>();
            var userManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null
            );

            userManager.Setup(u => u.FindByEmailAsync(userEmail))
                       .ReturnsAsync(fakeUser);

            userManager.Setup(u => u.IsInRoleAsync(fakeUser, RoleNames.Admin))
                       .ReturnsAsync(false);
            userManager.Setup(u => u.IsInRoleAsync(fakeUser, RoleNames.Associated))
                       .ReturnsAsync(false);

            // Mock de SignInManager
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var signInManager = new Mock<SignInManager<User>>(
                userManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null
            );

            signInManager.Setup(s => s.PasswordSignInAsync(
                fakeUser.UserName, userPassword, true, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Mock del logger
            var logger = new Mock<ILogger<AccountController>>();

            // Controlador
            var controller = new AccountController(signInManager.Object, userManager.Object, logger.Object);

            // Act
            var result = await controller.Login(userEmail, userPassword);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // 🔧 Arrange
            var mockSignInManager = new Mock<SignInManager<User>>(
                new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null).Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);

            var mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            var mockLogger = new Mock<ILogger<AccountController>>();

            // Simular que no encuentra usuario
            mockUserManager.Setup(x => x.FindByEmailAsync("wrong@example.com"))
                .ReturnsAsync((User)null);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object, mockLogger.Object);

            // ⚙️ Act
            var result = await controller.Login("wrong@example.com", "badpassword");

            // 🔍 Assert
            var viewResult = Assert.IsType<ViewResult>(result); // Debe regresar la vista Login
            Assert.False(controller.ModelState.IsValid); // ModelState inválido
            Assert.Contains(controller.ModelState, kvp => kvp.Value.Errors.Count > 0); // Contiene mensaje de error
        }
    }
}
