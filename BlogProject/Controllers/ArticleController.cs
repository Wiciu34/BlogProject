using BlogProject.Models;
using BlogProject.Repository.Interfaces;
using BlogProject.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers;


public class ArticleController : Controller
{
    private readonly IArticleRepository articleRepository;
    private readonly IPhotoService photoService;

    public ArticleController(IArticleRepository articleRepository, IPhotoService photoService)
    {
        this.articleRepository = articleRepository;
        this.photoService = photoService;
    }
    public async Task<IActionResult> Index(string category, string searchString, int? pageNumber)
    {
        int pageSize = 6;

        var articles = articleRepository.GetAllAsync();

        var categoryQuery = articles.Select(x => x.Category).Distinct().OrderBy(category => category);

        if (!String.IsNullOrEmpty(searchString))
        {
            articles = articles.Where(a =>
                EF.Functions.Like(a.Title, $"%{searchString}%"));
        }


        if (!String.IsNullOrEmpty(category))
        {
            articles = articles.Where(x => x.Category == category);
        }

        var paginatedArticles = await PaginatedList<Article>.CreateAsync(articles, pageNumber?? 1, pageSize);

        var articleViewModel = new ArticleCategoryViewModel
        {
            articles = paginatedArticles,
            Categories =  new SelectList(categoryQuery.ToList())
        };

        return View(articleViewModel);
    }

    public async Task<IActionResult> Detail(int id)
    {
        Article? article = await articleRepository.GetById(id);   
        return View(article);
    }

    [Authorize(Roles = "admin")]
    public IActionResult Create() 
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateArticleViewModel articleViewModel)
    {
        if (ModelState.IsValid) 
        {
            if (articleViewModel.Image != null)
            {
                var result = await photoService.AddPhotoAsync(articleViewModel.Image);

                var article = new Article
                {
                    Title = articleViewModel.Title,
                    Content = articleViewModel.Content,
                    Category = articleViewModel.Category,
                    Image = result.Url.ToString()
                };

                await articleRepository.Add(article);
                return RedirectToAction("Detail", "Article", new { id = article.Id });
            }
            else
            {
                var article = new Article
                {
                    Title = articleViewModel.Title,
                    Content = articleViewModel.Content,
                    Category = articleViewModel.Category,
                    Image = ""
                };

                await articleRepository.Add(article);
                return RedirectToAction("Detail", "Article", new { id = article.Id });
            }   
           
        }
        else
        {
            ModelState.AddModelError("", "Photo upload failed");
        }

        return View(articleViewModel);
       
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(int id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var article = await articleRepository.GetById(id);

        if (article == null)
        {
            return View("Error");
        }

        var articleVM = new EditArticleViewModel
        {
            Title = article.Title,
            Content = article.Content,  
            Category = article.Category,
            URL = article.Image
        };
        return View(articleVM);
        
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditArticleViewModel articleVM)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Failed to edit club");
            return View(articleVM);
        }

        var article = await articleRepository.GetById(id);

        if (article == null)
        {
            return View("Error");
        }

        if (articleVM.Image != null)
        {
            var photoResult = await photoService.AddPhotoAsync(articleVM.Image);

            if (photoResult.Error != null)
            {
                ModelState.AddModelError("Image", "Photo upload failed");
                return View(articleVM);
            }

            if (!string.IsNullOrEmpty(article.Image))
            {
                _ = photoService.DeletePhotoAsync(article.Image);
            }
            article.Image = photoResult.Url.ToString();
        }
        else
        {
            article.Image = "";
        }
    
        article.Title = articleVM.Title;
        article.Content = articleVM.Content;
        article.Category = articleVM.Category;

        await articleRepository.Update(article);

        return RedirectToAction("Detail", "Article", new { id });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Delete(int articleId) 
    {
        await articleRepository.Delete(articleId);
        return RedirectToAction("Index");
    }
}
