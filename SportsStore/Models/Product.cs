using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportsStore.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Enter a product name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Enter a description")]
        public string Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Enter a positive price")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Specify a category")]
        public string Category { get; set; }
    }
}
