using System.ComponentModel.DataAnnotations;

namespace Interview.Models
{
    public class CreateUserRequest
    {
        public CreateUserRequest(string name, string email, List<string> roleCodes)
        {
            Name = name;
            Email = email;
            RoleCodes = roleCodes;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> RoleCodes { get; set; }

    }


}