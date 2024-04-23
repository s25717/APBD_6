using Microsoft.EntityFrameworkCore;
using APBD_warehouse.Models;

namespace APBD_warehouse.Controllers
{
    public class WarehouseDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductWarehouse> ProductWarehouses { get; set; }

        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options)
        {

        }
    }
}
