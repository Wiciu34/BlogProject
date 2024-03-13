using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Content { get; set; }
    public Article? Article { get; set; }
    public AppUser? AppUser { get; set; }
}
