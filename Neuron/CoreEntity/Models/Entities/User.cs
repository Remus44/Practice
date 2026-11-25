using Microsoft.EntityFrameworkCore;

namespace Interview.Models
{
    public class User: BaseEntity
    {
        public User(string name, string email, HashSet<Role> roles)
        {
            Name = name;
            Email = email;
            Roles = roles;
        }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public HashSet<Role>? Roles { get; set; }     
    } 
}