using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Interview.Models
{
    public class UserDTO : BaseDTO
    {

        public UserDTO(string name, string email, Collection<RoleDTO> roles)
        {
            Name = name;
            Email = email;
            Roles = roles;

        }
        
        public string Name { get; set; }
        public string Email { get; set; }
        public Collection<RoleDTO>? Roles { get; set; }
    }
}