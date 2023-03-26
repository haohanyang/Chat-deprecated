using Chat.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Chat.Server.Data;

public class ApplicationDbContext : IdentityUserContext<IdentityUser>
{

    public DbSet<Group> Groups { get; set; }
    public DbSet<UserMessage> UserMessages { get; set; }
    public DbSet<GroupMessage> GroupMessages { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Membership> Memberships { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=:memory;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Group>().ToTable("Group");
        modelBuilder.Entity<Member>().ToTable("Member");
        
        modelBuilder.Entity<Membership>().Property(e => e.MembershipId).ValueGeneratedOnAdd();
        modelBuilder.Entity<Membership>().ToTable("Membership");
        modelBuilder.Entity<UserMessage>().ToTable("UserMessage");
        modelBuilder.Entity<GroupMessage>().ToTable("GroupMessage");
        base.OnModelCreating(modelBuilder);
    }

}