using Interview.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
namespace Interview.Repositories
{

    public class UserRepository
    {

        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public User Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }


        public User Update(User updatedUser)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);

            if (existingUser == null)
            {
                return null;
            }
            _context.Entry(existingUser).CurrentValues.SetValues(updatedUser);
            _context.SaveChanges();

            return updatedUser;
        }

        public User GetUser(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id); ;
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

    }
}