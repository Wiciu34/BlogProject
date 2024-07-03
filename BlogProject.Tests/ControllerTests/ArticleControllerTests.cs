using BlogProject.Controllers;
using BlogProject.Models;
using BlogProject.Repository.Interfaces;
using BlogProject.ViewModel;
using CloudinaryDotNet.Actions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MockQueryable.FakeItEasy;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlogProject.Tests.ControllerTests
{
    public class ArticleControllerTests
    {
        private IArticleRepository _articleRepository;
        private IPhotoService _photoService;
        private ISemaphoreService _semaphoreService;
        private ArticleController _articleController;
        public ArticleControllerTests() 
        {
            //Dependencise
            _articleRepository = A.Fake<IArticleRepository>();
            _photoService = A.Fake<IPhotoService>();
            _semaphoreService = A.Fake<ISemaphoreService>();

            //SUT
            _articleController = new ArticleController(_articleRepository, _photoService, _semaphoreService);
        }

        //Index

        [Fact]
        public async Task Index_ReturnsSucces()
        {
            //Arrange - Whta do I need to bring in?
            var articles = new List<Article>
            {
                new Article { Title = "Test Article 1", Category = "Tech" },
                new Article { Title = "Test Article 2", Category = "Health" },
                new Article { Title = "Test Article 3", Category = "Tech" },
                new Article { Title = "Test Article 4", Category = "Lifestyle" },
            };

            var mock = articles.BuildMock();

            A.CallTo(() => _articleRepository.GetAllAsync()).Returns(mock);

            string? category = null;
            string? searchString = null;
            int? pageNumber = 1;

            //Act
            var result = await _articleController.Index(category, searchString, pageNumber) as ViewResult;

            //Assert

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<ArticleCategoryViewModel>(result.Model);
            Assert.Equal(3, model.Categories.Count());
            Assert.Equal(4, model.articles.Items.Count());
        }

        //Detail

        [Fact]
        public async Task Detail_ReturnSucces()
        {
            //Arrange
            var article = new Article { Id = 1, Title = "title", Category = "Tech", Content = "content" };
            A.CallTo(() => _articleRepository.GetById(article.Id)).Returns(article);

            //Act
            var resulut = await _articleController.Detail(article.Id) as ViewResult;

            //Assert
            Assert.NotNull(resulut);
            var model = Assert.IsAssignableFrom<Article>(resulut.Model);
            Assert.Equal("title", model.Title);
            Assert.Equal("content", model.Content);
            Assert.Equal("Tech", model.Category);
        }

        //Create

        [Fact]
        public async Task Create_ValidModelWithImage_RedirectToDetail()
        {
            //Arrange
            var fileBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            var fileName = "test-image.jpg";
            var formFile = new FormFile(new System.IO.MemoryStream(fileBytes), 0, fileBytes.Length, "Data", fileName);

            var articleViewModel = new CreateArticleViewModel
            {
                Title = "Test Article",
                Content = "content",
                Category = "category",
                Image = formFile
            };

            A.CallTo(() => _photoService.AddPhotoAsync(A<IFormFile>.Ignored)).Returns(Task.FromResult(new ImageUploadResult { Url = new Uri("http://example.com/image.jpg") }));

            //Act
            var result = await _articleController.Create(articleViewModel) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Detail", result.ActionName);
            Assert.NotNull(result.RouteValues["id"]);
            A.CallTo(() => _articleRepository.Add(A<Article>.Ignored)).MustHaveHappenedOnceExactly();

        }

        [Fact]
        public async Task Create_ValidModelWithoutImage_RedirectToDetail()
        {
            //Arrange
          
            var articleViewModel = new CreateArticleViewModel
            {
                Title = "Test Article",
                Content = "content",
                Category = "category",
                Image = null
            };

            //Act
            var result = await _articleController.Create(articleViewModel) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Detail", result.ActionName);
            Assert.NotNull(result.RouteValues["id"]);
            A.CallTo(() => _articleRepository.Add(A<Article>.Ignored)).MustHaveHappenedOnceExactly();

        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsViewWithError()
        {
            //Arrange
            var articleViewModel = new CreateArticleViewModel
            {
                Title = "Test Article",
                Content = "content",
                Category = "category",
                Image = null
            };

            _articleController.ModelState.AddModelError("Title", "Title is required");     

            //Act
            var result = await _articleController.Create(articleViewModel) as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(articleViewModel, result.Model);
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.True(result.ViewData.ModelState.Count > 0);
            A.CallTo(() => _articleRepository.Add(A<Article>.Ignored)).MustNotHaveHappened();

        }

        //Edit

        [Fact]
        public async Task Edit_SemaphoreIsAlreadyOccupied_ReturnsRedirectToDetailWithWarning()
        {
            //Arrange
            int id = 1;
            var semaphore = new SemaphoreSlim(0, 1);

            A.CallTo(() => _articleRepository.GetById(id)).Returns(new Article { Id = id });
            A.CallTo(() => _semaphoreService.findSemaphore(id)).Returns(semaphore);

            var controller = _articleController;
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            //Act

            var result = await controller.Edit(id) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Detail", result.ActionName);
            Assert.Equal(id, result.RouteValues["id"]);
            Assert.Equal("This article is edited or deleted now", _articleController.TempData["Warning"]);

        }

        [Fact]
        public async Task Edit_ArticleExists_ReturnsEditView()
        {
            //Arrange
            int id = 1;
            var article = new Article { Id = id, Title = "Test title",  Content = "Test Content", Category = "category", Image = ""};
            A.CallTo(() => _articleRepository.GetById(id)).Returns(Task.FromResult(article));
            
            var semaphore = new SemaphoreSlim(1, 1);
            A.CallTo(() => _semaphoreService.findSemaphore(id)).Returns(semaphore);

            //Act

            var result = await _articleController.Edit(id) as ViewResult;

            //Assert

            Assert.NotNull(result);

            var model = Assert.IsType<EditArticleViewModel>(result.Model);

            Assert.Equal(id, model.Id);
            Assert.Equal("Test title", model.Title);
            Assert.Equal("Test Content", model.Content);
            Assert.Equal("category", model.Category);
            Assert.Equal("", model.URL);
        }

        //Delete

        [Fact]
        public async Task Delete_ArticleFound_ReturnsViewWithArticle()
        {
            //Arrange
            int id = 1;
            var article = new Article { Id = id };
            var semaphore = new SemaphoreSlim(1, 1);

            A.CallTo(() => _semaphoreService.findSemaphore(id)).Returns(semaphore);
            A.CallTo(() => _articleRepository.GetById(id)).Returns(Task.FromResult(article));

            //Act
            var  result = await _articleController.Delete(id) as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(article, result.Model);

        }

        [Fact]
        public async Task DeleteConfirmed_ArticleDeleted_RedirectsToIndex()
        {
            //Arrange
            int id = 1;
            var semaphore = new SemaphoreSlim(0, 1);

            A.CallTo(() => _semaphoreService.findSemaphore(id)).Returns(semaphore);

            //Act
            var result = await _articleController.DeleteConfirmed(id) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            A.CallTo(() => _articleRepository.Delete(id)).MustHaveHappenedOnceExactly();
            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
