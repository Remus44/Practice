using Microsoft.EntityFrameworkCore;

namespace Interview.Models
{
    public class Role: BaseEntity
    {
        public Role(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        public string Code { get; set; }
        public HashSet<Permission>? Permissions { get; set; }
    }
}