using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Repository;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Tests.RepositoryTests;

public class CommentRepositoryTests
{
    private async Task<BlogDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new BlogDbContext(options);
        databaseContext.Database.EnsureCreated();

        await databaseContext.Comments.AddAsync(
                new Comment
                {
                    Id = 1,
                    Content = "Test content 1",
                    Article = new Article { Id = 1, Title = "t1", Content = "c1", Category = "category 1" },
                    AppUser = new AppUser { Id = "1", UserName = "TestUser1" }
                }
            ) ;

        await databaseContext.Comments.AddAsync(
                new Comment
                {
                    Id = 2,
                    Content = "Test content 2",
                    Article = new Article { Id = 2, Title = "t2", Content = "c2", Category = "category 2" },
                    AppUser = new AppUser { Id = "2", UserName = "TestUser2" }
                }
            );

        await databaseContext.SaveChangesAsync();

        return databaseContext;
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectComment()
    {
        //Arrange
        var dbContext = await GetDbContext();
        var commentRepository = new CommentRepository(dbContext);

        //Act
        var result = await commentRepository.GetByIdAsync( 1 );

        //Assert
        Assert.NotNull( result );
        Assert.Equal(1, result.Id);
        Assert.Equal("Test content 1", result.Content); 
    }

    [Fact]
    public async Task Add_SavesCommentToDatabase()
    {
        //Arrange
        var dbContext = await GetDbContext();
        var commentRepository = new CommentRepository(dbContext);

        var newComment = new Comment
        {
            Id = 3,
            Content = "Test content 3",
            Article = new Article { Id = 3, Title = "t2", Content = "c2", Category = "category 2" },
            AppUser = new AppUser { Id = "3", UserName = "TestUser2" }
        };

        //Act
        await commentRepository.Add(newComment);

        //Assert
        var savedComment = await dbContext.Comments.FindAsync(newComment.Id);
        Assert.NotNull( savedComment );
        Assert.Equal(newComment.Content, savedComment.Content);
    }

    [Fact]
    public async Task Delete_RemovesCommentFromDatabase()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var commentRepository = new CommentRepository(dbContext);
        var commentIdToDelete = 1;

        // Act
        await commentRepository.Delete(commentIdToDelete);

        // Assert
        var deletedComment = await dbContext.Comments.FindAsync(commentIdToDelete);
        Assert.Null(deletedComment);
    }
}
