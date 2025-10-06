using Cart.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Cart.API.Data;

public class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}