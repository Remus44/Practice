using System;
using System.Collections.Generic;
using Interview.Models;
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


        public bool Update(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (existingUser != null)
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);

                _context.SaveChanges();
                return true; ;
            }
            return false;
        }

    }
}