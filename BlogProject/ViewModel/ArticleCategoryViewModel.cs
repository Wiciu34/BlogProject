using BlogProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlogProject.ViewModel;

public class ArticleCategoryViewModel
{
    public PaginatedList<Article>? articles {  get; set; }
    public SelectList? Categories {  get; set; }
    public string? Category { get; set; }
    public string? SearchString { get; set; }
}
