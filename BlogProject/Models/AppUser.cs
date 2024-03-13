using Microsoft.AspNetCore.Identity;

namespace BlogProject.Models;

public class AppUser : IdentityUser
{
    public ICollection<Comment>? Comments { get; set; }
}
