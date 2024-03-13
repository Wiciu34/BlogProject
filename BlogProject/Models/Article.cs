using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models;

public class Article
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Title { get; set; }
    [Required]
    public string? Content { get; set; }
    [Required]
    public string? Category { get; set; }
    public string? Image {  get; set; }
    public ICollection<Comment>? Comments { get; set; }
}
