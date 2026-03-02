using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DrillType> DrillTypes => Set<DrillType>();
    public DbSet<Drill> Drills => Set<Drill>();
    public DbSet<PracticePlan> PracticePlans => Set<PracticePlan>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<PlanDrill> PlanDrills => Set<PlanDrill>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Rename Identity framework tables (no custom classes to nest config into)
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
    }
}
