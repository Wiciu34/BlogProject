using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Repository.Interfaces;

namespace BlogProject.Repository;

public class CommentRepository : ICommentRepository
{
    private readonly BlogDbContext context;

    public CommentRepository(BlogDbContext context)
    {
        this.context = context;
    }

    public async Task<Comment> GetByIdAsync(int id)
    {
        return await context.Comments.FindAsync(id);
    }

    public async Task Add(Comment comment)
    {
        context.Add(comment);
        await context.SaveChangesAsync();
        
    }

    public async Task Delete(int commentId)
    {
        var existingComment = await context.Comments.FindAsync(commentId);

        if (existingComment != null)
        {
            context.Remove(existingComment);
            await context.SaveChangesAsync();
        }
    }


}
