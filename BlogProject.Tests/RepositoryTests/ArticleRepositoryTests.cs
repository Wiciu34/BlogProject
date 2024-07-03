using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlogProject.Tests.RepositoryTests;

public class ArticleRepositoryTests
{
    private async Task<BlogDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new BlogDbContext(options);
        databaseContext.Database.EnsureCreated();

        await databaseContext.Articles.AddAsync(new Article { Id = 1, Title= "Test Article 1", Category = "category 1", Content = "content 1"});
        await databaseContext.Articles.AddAsync(new Article { Id = 2, Title = "Test Article 2", Category = "category 2", Content = "content 2" });

        await databaseContext.SaveChangesAsync();

        return databaseContext;
    }

    [Fact]
    public async Task GetAllAsync_ReturnQueryableList()
    {
        //Arrange
        var dbContext = await GetDbContext();
        var articleRepository = new ArticleRepository(dbContext);

        //Act
        var result = articleRepository.GetAllAsync();

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable<Article>>(result);

        var articleList = await result.ToListAsync();
        Assert.NotEmpty(articleList);
        Assert.Equal(2, articleList.Count());

    }

    [Fact]
    public async Task GetById_ReturnsArticle()
    {
        //Arrange 
        var id = 1;
        var dbContext = await GetDbContext();
        var articleRepository = new ArticleRepository(dbContext);

        //Act 
        var result = articleRepository.GetById(id);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<Task<Article>>(result);
    }

    [Fact]
    public async Task Add()
    {
        //Arrange
        var article = new Article
        {
            Title = "Test",
            Category = "Category",
            Content = "content"
        };

        var dbContext = await GetDbContext();
        var articleRepository = new ArticleRepository(dbContext);

        //Act
        var result = articleRepository.Add(article);

        //Assert
        var savedArticle = await dbContext.Articles.FindAsync(article.Id);
        Assert.NotNull(savedArticle);
        Assert.Equal(article.Title, savedArticle.Title);
    }

    [Fact]
    public async Task Update()
    {
        //Arrange
        var dbContext = await GetDbContext();
        var articleRepository = new ArticleRepository(dbContext);

        var articleToUpdate = await dbContext.Articles.FindAsync(1);
        var originalTitle = articleToUpdate.Title;
        articleToUpdate.Title = "Updated Title";

        //Act
        await articleRepository.Update(articleToUpdate);

        //Assert
        var updatedArticle = await dbContext.Articles.FindAsync(1);
        Assert.NotNull(updatedArticle);
        Assert.NotEqual(originalTitle, updatedArticle.Title);
        Assert.Equal("Updated Title", updatedArticle.Title);
    }

    [Fact]
    public async Task Delete()
    {
        //Arrange
        var dbContext = await GetDbContext();
        var articleRepository = new ArticleRepository(dbContext);

        var articleIdToDelete = 1;

        //Act
        await articleRepository.Delete(articleIdToDelete);

        //Assert
        var deletedArticle = await dbContext.Articles.FindAsync(articleIdToDelete);
        Assert.Null(deletedArticle);
    }
}
