using BlogProject.Controllers;
using BlogProject.Models;
using BlogProject.ViewModel;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BlogProject.Tests.ControllerTests;

public class AccountControllerTests
{
    private UserManager<AppUser> _userManager;
    private SignInManager<AppUser> _signInManager;
    private AccountController _accountController;

    public AccountControllerTests()
    {
        _userManager = A.Fake<UserManager<AppUser>>();
        _signInManager = A.Fake<SignInManager<AppUser>>();

        _accountController = new AccountController(_userManager, _signInManager);
    }

    //Login

    [Fact]
    public async Task Login_ReturnsRedirectToHomeIndex_WhenLoginSucceeds()
    {
        //Arrange
        var loginViewModel = new LoginViewModel { Email = "test@example.com", Password = "Password123!" };
        var user = new AppUser { Email = "test@example.com" };

        A.CallTo(() => _userManager.FindByEmailAsync(loginViewModel.Email)).Returns(user);
        A.CallTo(() => _userManager.CheckPasswordAsync(user, loginViewModel.Password)).Returns(true);
        A.CallTo(() => _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false)).Returns(SignInResult.Success);
        
        //Act
        var result = await _accountController.Login(loginViewModel);

        //Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);

    }

    [Fact]
    public async Task Login_ReturnsViewResult_WithModel_WhenLoginFails()
    {
        // Arrange
        var loginViewModel = new LoginViewModel { Email = "test@example.com", Password = "Password123!" };
        var user = new AppUser { Email = "test@example.com" };
        A.CallTo(() => _userManager.FindByEmailAsync(loginViewModel.Email)).Returns(user);
        A.CallTo(() => _userManager.CheckPasswordAsync(user, loginViewModel.Password)).Returns(false);

        var controller = _accountController;
        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );

        // Act
        var result = await controller.Login(loginViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(loginViewModel, viewResult.Model);
        Assert.Equal("Wrong credentials. Please try again", _accountController.TempData["Error"]);
    }

    [Fact]
    public async Task Login_ReturnsViewResult_WithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var loginViewModel = new LoginViewModel(); // ModelState will be invalid
        _accountController.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _accountController.Login(loginViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(loginViewModel, viewResult.Model);
    }

    //Register

    [Fact]
    public async Task Register_ReturnsRegistrationConfirmation_WhenRegistrationSucceeds()
    {
        // Arrange
        var registerViewModel = new RegisterViewModel
        {
            Email = "test@example.com",
            Name = "TestUser",
            Password = "Password123!"
        };
        A.CallTo(() => _userManager.FindByEmailAsync(registerViewModel.Email)).Returns((AppUser)null); // No existing user with this email
        A.CallTo(() => _userManager.CreateAsync(A<AppUser>.Ignored, registerViewModel.Password)).Returns(IdentityResult.Success);

        // Act
        var result = await _accountController.Register(registerViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RegistrationConfirmation", viewResult.ViewName);
    }

    [Fact]
    public async Task Register_ReturnsViewResult_WithModel_WhenEmailIsAlreadyTaken()
    {
        // Arrange
        var registerViewModel = new RegisterViewModel
        {
            Email = "test@example.com",
            Name = "TestUser",
            Password = "Password123!"
        };
        var existingUser = new AppUser { Email = registerViewModel.Email };
        A.CallTo(() => _userManager.FindByEmailAsync(registerViewModel.Email)).Returns(existingUser);

        var controller = _accountController;
        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );

        // Act
        var result = await controller.Register(registerViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(registerViewModel, viewResult.Model);
        Assert.Equal("This email address is already in use", _accountController.TempData["Error"]);
    }

    [Fact]
    public async Task Register_ReturnsViewResult_WithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var registerViewModel = new RegisterViewModel(); // Invalid ModelState
        _accountController.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _accountController.Register(registerViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(registerViewModel, viewResult.Model);
    }

    //Logout

    [Fact]
    public async Task Logout_ReturnsRedirectToAction_Index_Article()
    {
        // Arrange

        // Act
        var result = await _accountController.Logout();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Article", redirectResult.ControllerName);

        // Verify that SignOutAsync() was called on signInManager
        A.CallTo(() => _signInManager.SignOutAsync()).MustHaveHappenedOnceExactly();
    }

}
