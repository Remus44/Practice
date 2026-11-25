using AutoMapper;
using Interview.Models;
using Interview.Repositories;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.ObjectModel;
using System.Linq;
namespace Interview.Services
{

    public class RoleService
    {

        private readonly ILogger<RoleService> _logger;
        private RoleRepository _roleRepository;
        private PermissionRepository _permissionRepository;
        private readonly IMapper _mapper;
        private IUnitOfWork _unitOfWork;


        public RoleService(ILogger<RoleService> logger, IMapper mapper, IUnitOfWork unitOfWork, RoleRepository roleRepository, PermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
        public async Task<Collection<RoleDTO>> CreateUpdateRolesAndPermissionsAsync(Collection<RoleDTO>? roles)
        {
            Collection<RoleDTO> result = new Collection<RoleDTO>();
            if (roles == null || roles.Count == 0)
            {
                // walk the roles
                foreach (var role in roles)
                {
                    await CreateUpdateRolesAndPermissionsTransaction(role);
                    result.Add(role);
                }
            }            
            return result;
        }

        public async Task CreateUpdateRolesAndPermissionsTransaction(RoleDTO role)
        {
            // check if role exists
            bool roleExist = _roleRepository.GetByCode(role.Code) != null;

            // collect missing permissions
            List<Permission> permissionsToCreate = new List<Permission>();
            foreach (var permission in role.Permissions)
            {
                var permissionFound = _permissionRepository.GetByCode(permission.Code);
                if (permissionFound == null)
                {
                    permissionsToCreate.Add(permissionFound);
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var item in permissionsToCreate)
                {
                    await _permissionRepository.CreateAsync(item);                    
                }

                if (roleExist) 
                { 
                    await _roleRepository.CreateAsync(_mapper.Map<Role>(role));
                }
                else
                {
                    await _roleRepository.UpdateAsync(_mapper.Map<Role>(role));
                }
                
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}