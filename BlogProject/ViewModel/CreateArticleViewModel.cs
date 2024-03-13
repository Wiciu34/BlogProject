
namespace BlogProject.ViewModel;

public class CreateArticleViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; } 
    public string? Content { get; set; }
    public string? Category { get; set;}
    public IFormFile? Image { get; set; }
}
