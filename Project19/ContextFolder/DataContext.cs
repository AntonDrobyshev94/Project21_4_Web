using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project19.Entitys;
using Project19.AuthContactApp;

namespace Project19.ContextFolder
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext (DbContextOptions options): base(options) { }
        public DbSet<Contact> Contacts { get; set; } 
    }
}
