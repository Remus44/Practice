using Microsoft.EntityFrameworkCore;

namespace Interview.Models
{
    public class Permission: BaseEntity
    {
        public Permission(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        public string Code { get; set; }
    }
}