using System;
using System.Collections.ObjectModel;
using Interview.Models;
using Interview.Repositories;
namespace Interview.Services
{

    public class RoleService
    {

        private readonly ILogger<RoleService> _logger;
        private RoleRepository _roleRepository;
        private PermissionRepository _permissionRepository;

        public RoleService(ILogger<RoleService> logger, RoleRepository roleRepository, PermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /**
         * Szerepkörök és elemi jogok létrehozása, összekapcsolása, módosítása
         *
         * Megvizsgálja, hogy létezik-e már az adott szerepkör/elemi jog adatbázisban a {@link RoleDTO#code} illetve {@link PermissionDTO#code} mezőjük alapján.
         * Amennyiben nem létezik, akkor létrehozza őket.
         * A paraméterben érkező struktúrának megfelelően alakítja ki adatbázisban is a szerepkörök és az elemi jogok közötti kapcsolatot. 
         * Azaz, amennyiben szükséges, létrehozza a szerepköröket/elemi jogokat adatbázisban, illetve módosítja a szerepkörök és elemi jogok összerendelését úgy, 
         * hogy azonos legyen a paraméterben érkező struktúrával
         *
         * @param roles szerepkörök és a hozzájuk tartozó elemi jogok
         * @return a parméterben érkező szerepkörök frissített adatai
         */
        public Collection<RoleDTO> CreateUpdateRolesAndPermissions(Collection<RoleDTO>? roles)
        {
            return null;
        }

    }
}