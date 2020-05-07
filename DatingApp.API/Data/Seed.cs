using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager) 
        {
            if (!userManager.Users.Any())
            {
                foreach (var role in new List<Role> { new Role{ Name = "Member" }, new Role{ Name = "Admin" }, new Role{ Name = "Moderator" }, new Role{ Name = "VIP" } }) roleManager.CreateAsync(role).Wait();
                foreach (var user in JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText("Data/UserSeedData.json")))
                {
                    user.Photos.SingleOrDefault().IsApproved = true;
                    userManager.CreateAsync(user, "password").Wait();
                    userManager.AddToRoleAsync(user, "Member").Wait();
                }
                if (userManager.CreateAsync(new User { UserName = "Admin" }, "admin").Result.Succeeded) userManager.AddToRolesAsync(userManager.FindByNameAsync("Admin").Result, new[] { "Admin", "Moderator" });
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}