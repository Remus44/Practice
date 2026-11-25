using Interview.Models;
namespace Interview.Repositories
{
    public class RoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public Role? GetByCode(string code)
        {
            var existingRole = _context.Roles.FirstOrDefault(r => r.Code == code);
            if (existingRole != null)
            {
                _context.Entry(existingRole).Collection(r => r.Permissions).Load();
            }
            return existingRole;
        }

        public List<Role> GetallRoles()
        {
            var roles = _context.Roles;
            foreach (var role in roles)
            {
                _context.Entry(role).Collection(r => r.Permissions).Load();
            }
            return roles.ToList();
        }
                
        public void Create(Role role)
        {
            _context.Roles.Add(role);
            _context.SaveChanges();
        }

        public void Update(Role role)
        {
            _context.Roles.Update(role);
            _context.SaveChanges();
        }
    }
}