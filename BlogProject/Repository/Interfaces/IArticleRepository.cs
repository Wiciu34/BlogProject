using BlogProject.Models;

namespace BlogProject.Repository.Interfaces;

public interface IArticleRepository
{
    IQueryable<Article> GetAllAsync();
    Task<Article> GetById(int id);
    Task Add(Article article);
    Task Update(Article article);
    Task Delete(int id);

}
