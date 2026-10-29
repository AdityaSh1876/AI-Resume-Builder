using AIResumeBuilder.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIResumeBuilder.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Resume> Resumes => Set<Resume>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Resume>().Property(x => x.Title).HasMaxLength(120);
        builder.Entity<Resume>().Property(x => x.FullName).HasMaxLength(160);
        builder.Entity<Resume>().HasIndex(x => new { x.UserId, x.UpdatedAt });
    }
}
