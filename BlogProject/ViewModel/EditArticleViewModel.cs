namespace BlogProject.ViewModel;

public class EditArticleViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public string? URL { get; set; }
    public IFormFile? Image { get; set; }
}
