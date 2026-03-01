using Microsoft.EntityFrameworkCore;
namespace PasskeyHackathon2._0.Models
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options) { }

        public DbSet<PasskeyCredentialModel> PasskeyCredentials { get; set; }
        public DbSet<PasskeyChallengeModel> PasskeyChallenges { get; set; }
    }
}
