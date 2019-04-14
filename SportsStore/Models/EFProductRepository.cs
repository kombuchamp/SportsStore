using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsStore.Models
{
    public class EFProductRepository : IProductRepository
    {
        private ApplicationDbContext context;
        public EFProductRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IQueryable<Product> Products => context.Products;

        public Product DeleteProduct(int productID)
        {
            Product dbEntry = context.Products
                .FirstOrDefault(p => p.ProductID == productID);
            if (dbEntry != null)
            {
                try
                {
                    // It may fail because if product is ordered FK won't allow to delete it
                    context.Products.Remove(dbEntry);
                    context.SaveChanges();
                }
                catch
                {
                    throw new InvalidOperationException("Product was ordered! Unable to delete");
                }

            }
            return dbEntry;
        }

        public void SaveProduct(Product product)
        {
            if (product.ProductID == 0)
            {
                context.Products.Add(product);
            }
            else
            {
                Product dbEntry = context.Products
                    .FirstOrDefault(p => p.ProductID == product.ProductID);
                if (dbEntry != null)
                {
                    dbEntry.Name = product.Name;
                    dbEntry.Description = product.Description;
                    dbEntry.Price = product.Price;
                    dbEntry.Category = product.Category;
                }
            }
            context.SaveChanges();
        }
    }
}
