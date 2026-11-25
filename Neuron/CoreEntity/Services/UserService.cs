using AutoMapper;
using Interview.Models;
using Interview.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Transactions;
using System.Xml.Linq;
namespace Interview.Services
{

    public class UserService
    {

        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;

        public UserService(ILogger<UserService> logger, IMapper mapper, UserRepository userRepository, RoleRepository roleRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }
        /**
         * Új felhasználó létrehozása.
         * <p>
         * Létrehoz egy új felhasználót a paraméterben megkapott adatok alapján.
         * Hozzákapcsolja a felhasználóhoz a paraméterben érkező szerepköröket. 
         * Amennyiben nem létező szerepkör érkezik paraméterben, abban az esetben ArgumentException kivételt dob tartalmazva a nem létező szerepköröket.
         *
         * @param name felhasználó neve
         * @param roleCodes felhasználóhoz hozzáadandó szerepkörök
         * @return létrehozott felhasználó
         */
        public UserDTO CreateUser(string name, string email, List<string> roleCodes)
        {
            // Collect roles by input role codes
            var roles = _roleRepository.GetallRoles();
            HashSet<Role> usrRolesSet = new HashSet<Role>();
            foreach (var roleCode in roleCodes) 
            {
                var roleFound = roles.Where(r => r.Code.Equals(roleCode)).FirstOrDefault();
                // Role not found
                if (roleFound == null) 
                {
                    throw new ArgumentException();
                }
                usrRolesSet.Add(roleFound);
            }       
            
            // Prepare the new record and send
            User user = _userRepository.Create(new User(name, email, usrRolesSet));
            return _mapper.Map<UserDTO>(user);
        }

        public UserDTO GetUser(int id)
        {
            var user = _userRepository.GetUser(id);
            return _mapper.Map<UserDTO>(user);
        }

        public UserDTO UpdateUser(UpdateUserRequest updatedUser)
        {
            var resultUser = _userRepository.Update(_mapper.Map<User>(updatedUser));
            return _mapper.Map<UserDTO>(resultUser);
        }

        public void DeleteUserAsync(UserDTO user)
        {
           _userRepository.DeleteUser(_mapper.Map<User>(user));            
        }
    }
}