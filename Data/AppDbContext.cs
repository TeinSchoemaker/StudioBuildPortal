using Microsoft.EntityFrameworkCore;
using StudioBuildPortal.Models;

namespace StudioBuildPortal.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ProjectEntry> Projects => Set<ProjectEntry>();
}