using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsStore.Models
{
    public class AppUser : IdentityUser
    {
        public AppUser() : base()
        {
        }
        public AppUser(string userName) : base(userName)
        {
        }
        public decimal Money { get; set; }
    }
}
