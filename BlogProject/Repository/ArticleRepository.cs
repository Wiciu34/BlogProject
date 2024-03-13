using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Repository;

public class ArticleRepository : IArticleRepository
{
    private readonly BlogDbContext _context;
    public ArticleRepository(BlogDbContext context)
    {
        _context = context;
    }

    public IQueryable<Article> GetAllAsync()
    {
        return _context.Articles;
    }

    public async Task<Article> GetById(int id)
    {
        return await _context.Articles.Include(a => a.Comments).ThenInclude(c => c.AppUser).
        FirstOrDefaultAsync(article => article.Id == id);
    }

    public async Task Add(Article article)
    {
        _context.Add(article);
        await _context.SaveChangesAsync();  
    }

    public async Task Update(Article article)
    {
        _context.Update(article);
        await _context.SaveChangesAsync();

    }

    public async Task Delete(int id)
    {
        var existingArticle = _context.Articles.FirstOrDefault(article => article.Id == id);

        if (existingArticle != null)
        {
            _context.Remove(existingArticle);
            await _context.SaveChangesAsync();

        }
    }
}
