using BlogProject.Models;
using BlogProject.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers;

public class CommentController : Controller
{
    private readonly ICommentRepository commentRepository;
    private readonly IArticleRepository articleRepository;
    private readonly UserManager<AppUser> userManager;

    public CommentController(ICommentRepository commentRepository, IArticleRepository articleRepository, UserManager<AppUser> userManager)
    {
        this.commentRepository = commentRepository;
        this.articleRepository = articleRepository;
        this.userManager = userManager;
    }

    [Authorize(Roles = "admin, user")]
    public async Task<IActionResult> Create(int articleId)
    {
        var article = await articleRepository.GetById(articleId);

        if (article == null)
        {
            return NotFound();
        }

        var user = await userManager.GetUserAsync(User);

        var comment = new Comment
        {
            
            Article = article,
            AppUser = user

        };
        return View(comment);
    }

    [Authorize(Roles = "admin, user")]
    [HttpPost]
    public async Task<IActionResult> Create(int articleId, string userId, Comment comment)
    {   
        if (!ModelState.IsValid)
        {
            return View(comment);
        }

        var article = await articleRepository.GetById(articleId);

        var user = await userManager.FindByIdAsync(userId);

        comment.Article = article;
        comment.AppUser = user;

        await commentRepository.Add(comment);

        return RedirectToAction("Detail", "Article", new {id = articleId});
    }

    [Authorize(Roles = "admin, user")]
    [HttpPost]
    public async Task<IActionResult> Delete(int commentId, int articleId)
    {

        await   commentRepository.Delete(commentId);
        return RedirectToAction("Detail", "Article", new {id = articleId});
    }
}
