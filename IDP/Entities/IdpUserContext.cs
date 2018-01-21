using Microsoft.EntityFrameworkCore;

namespace IDP.Entities
{
    public class IdpUserContext : DbContext
    {
        public IdpUserContext(DbContextOptions<IdpUserContext> options)
           : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }
    }
}
