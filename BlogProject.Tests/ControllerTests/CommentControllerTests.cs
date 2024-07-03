using BlogProject.Controllers;
using BlogProject.Models;
using BlogProject.Repository.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogProject.Tests.ControllerTests;

public class CommentControllerTests
{
    private ICommentRepository _commentRepository;
    private IArticleRepository _articleRepository;
    private UserManager<AppUser> _userManager;
    private CommentController _commentController;

    public CommentControllerTests()
    {
        _articleRepository = A.Fake<IArticleRepository>();
        _commentRepository = A.Fake<ICommentRepository>();
        _userManager = A.Fake<UserManager<AppUser>>();

        _commentController = new CommentController(_commentRepository, _articleRepository, _userManager);
    }

    //Create

    [Fact]
    public async Task Create_Get_ReturnsNotFound_WhenArticleIsNull()
    {
        // Arrange
        A.CallTo(() => _articleRepository.GetById(A<int>.Ignored)).Returns(Task.FromResult<Article>(null));

        // Act
        var result = await _commentController.Create(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Get_ReturnsViewResult_WithComment_WhenArticleExists()
    {
        // Arrange
        var article = new Article();
        var user = new AppUser();
        A.CallTo(() => _articleRepository.GetById(A<int>.Ignored)).Returns(article);
        A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(user);

        // Act
        var result = await _commentController.Create(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Comment>(viewResult.Model);
        Assert.Equal(article, model.Article);
        Assert.Equal(user, model.AppUser);
    }

    [Fact]
    public async Task Create_Post_ReturnsViewResult_WithModelErrors_WhenModelStateIsInvalid()
    {
        // Arrange
        _commentController.ModelState.AddModelError("Error", "Model error");
        var comment = new Comment();

        // Act
        var result = await _commentController.Create(1, "1", comment);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(comment, viewResult.Model);
    }

    [Fact]
    public async Task Create_Post_RedirectsToDetail_WhenCommentIsCreated()
    {
        // Arrange
        var article = new Article { Id = 1 };
        var user = new AppUser { Id = "1" };
        var comment = new Comment();
        A.CallTo(() => _articleRepository.GetById(A<int>.Ignored)).Returns(article);
        A.CallTo(() => _userManager.FindByIdAsync(A<string>.Ignored)).Returns(user);
        A.CallTo(() => _commentRepository.Add(A<Comment>.Ignored)).DoesNothing();

        // Act
        var result = await _commentController.Create(1, "1", comment);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Detail", redirectResult.ActionName);
        Assert.Equal("Article", redirectResult.ControllerName);
        Assert.Equal(article.Id, redirectResult.RouteValues["id"]);
    }

    //Delete

    [Fact]
    public async Task Delete_ReturnsRedirectToAction_WhenAdminDeletesComment()
    {
        // Arrange
        int commentId = 1;
        int articleId = 2;

        // Act
        var result = await _commentController.Delete(commentId, articleId);

        // Assert
        A.CallTo(() => _commentRepository.Delete(commentId)).MustHaveHappenedOnceExactly();
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Detail", redirectResult.ActionName);
        Assert.Equal("Article", redirectResult.ControllerName);
        Assert.Equal(articleId, redirectResult.RouteValues["id"]);
    }



}
