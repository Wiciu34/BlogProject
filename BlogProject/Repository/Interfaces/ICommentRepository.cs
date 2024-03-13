using BlogProject.Models;

namespace BlogProject.Repository.Interfaces;

public interface ICommentRepository
{
    Task<Comment> GetByIdAsync(int id);
    Task Add(Comment comment);
    Task Delete(int commentId);
}
