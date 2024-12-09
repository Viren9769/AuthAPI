using AuthAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Context
{
    public class AddDbContext: DbContext
    {
        public AddDbContext(DbContextOptions<AddDbContext> options):base(options) { }
        

        public DbSet<User> Users { get; set; }

        // we also need to send this record to table so create a OnmodelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //whatever name we give here it get same created in database 
            modelBuilder.Entity<User>().ToTable("users");
        }


    }
}


//A DbContext in ASP.NET serves as a bridge between your application and the database, managing connections, CRUD operations, and change tracking.
//It simplifies database interactions using LINQ queries, ensures data consistency with SaveChanges(), and supports Dependency Injection for scalability.