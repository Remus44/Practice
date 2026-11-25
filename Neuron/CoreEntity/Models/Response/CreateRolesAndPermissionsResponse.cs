namespace Interview.Models
{
    public class CreateRolesAndPermissionsResponse
    {
        public CreateRolesAndPermissionsResponse(IEnumerable<RoleDTO> roles)
        {
            Roles = roles;
        }

        public IEnumerable<RoleDTO> Roles { get; set; }
    }
}