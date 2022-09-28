using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime LastLogin { get; set; }
        public DateTime Created { get; set; }
        public bool Blocked { get; set; }
    }
}
