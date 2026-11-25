using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Interview.Models
{
    public class CreateUpdateRolesAndPermissionsRequest
    {
        public CreateUpdateRolesAndPermissionsRequest(Collection<RoleDTO> roles)
        {
            Roles = roles;
        }

        public Collection<RoleDTO> Roles { get; set; }
    }
}