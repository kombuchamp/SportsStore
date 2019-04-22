using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Enter positive amount of money")]
        public decimal Money { get; set; }
    }
}
