using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyFace.Data;
using MyFace.Models.Database;
using MyFace.Models.Request;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace MyFace.Repositories
{
    public interface IUsersRepo
    {
        IEnumerable<User> Search(SearchRequest search);
        int Count(SearchRequest search);
        User GetById(int id);
        User Create(CreateUserRequest newUser);
        User Update(int id, UpdateUserRequest update);
        void Delete(int id);
    }
    
    public class UsersRepo : IUsersRepo
    {
        private readonly MyFaceDbContext _context;

        public UsersRepo(MyFaceDbContext context)
        {
            _context = context;
        }
        
        public IEnumerable<User> Search(SearchRequest search)
        {
            return _context.Users
                .Where(p => search.Search == null || 
                            (
                                p.FirstName.ToLower().Contains(search.Search) ||
                                p.LastName.ToLower().Contains(search.Search) ||
                                p.Email.ToLower().Contains(search.Search) ||
                                p.Username.ToLower().Contains(search.Search)
                            ))
                .OrderBy(u => u.Username)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize);
        }

        public int Count(SearchRequest search)
        {
            return _context.Users
                .Count(p => search.Search == null || 
                            (
                                p.FirstName.ToLower().Contains(search.Search) ||
                                p.LastName.ToLower().Contains(search.Search) ||
                                p.Email.ToLower().Contains(search.Search) ||
                                p.Username.ToLower().Contains(search.Search)
                            ));
        }

        public User GetById(int id)
        {
            return _context.Users
                .Single(user => user.Id == id);
        }

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);
            return salt;

        }
        public string HashPassword(string password, string salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 5000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
    

        public User Create(CreateUserRequest newUser)
        {
            var salt = GenerateSalt();
            
            var insertResponse = _context.Users.Add(new User
            {
                
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Username = newUser.Username,
                ProfileImageUrl = newUser.ProfileImageUrl,
                CoverImageUrl = newUser.CoverImageUrl,
                Salt = salt,
                HashedPassword = HashPassword(newUser.PasswordInput, salt)
            });
            _context.SaveChanges();

            return insertResponse.Entity;
        }

        public User Update(int id, UpdateUserRequest update)
        {
            var user = GetById(id);

            user.FirstName = update.FirstName;
            user.LastName = update.LastName;
            user.Username = update.Username;
            user.Email = update.Email;
            user.ProfileImageUrl = update.ProfileImageUrl;
            user.CoverImageUrl = update.CoverImageUrl;

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(int id)
        {
            var user = GetById(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}