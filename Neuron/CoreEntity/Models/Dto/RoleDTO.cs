using System.Collections.Generic;

namespace Interview.Models
{
    public class RoleDTO:BaseDTO
    {

        public RoleDTO(string code, string name, IEnumerable<PermissionDTO> permissions)
        {
            Code = code;
            Name = name;
            Permissions = permissions;
        }

        public string Name { get; set; }
        public string Code { get; set; }
        public IEnumerable<PermissionDTO> Permissions { get; set; }
    }
}