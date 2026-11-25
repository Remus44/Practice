using Interview.Models;

namespace Interview.Repositories
{
    public class PermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Permission? GetByCode(string code)
        {
            return _context.Permissions.FirstOrDefault(p => p.Code == code);
        }

        public List<Permission>? GetAllPermissions()
        {
            return _context.Permissions.ToList();
        }

        public void Create(Permission permission)
        {
            _context.Permissions.Add(permission);
            _context.SaveChanges();
        }

        public async Task CreateAsync(Permission permission)
        {
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
        }

        public void Update(Permission permission)
        {
            _context.Permissions.Update(permission);
            _context.SaveChanges();
        }
    }
}