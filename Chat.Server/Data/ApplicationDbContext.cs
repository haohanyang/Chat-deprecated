using Chat.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Data;

public class ApplicationDbContext : IdentityUserContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Group> Groups { get; set; }
    public DbSet<UserMessage> UserMessages { get; set; }
    public DbSet<GroupMessage> GroupMessages { get; set; }
    public DbSet<Membership> Memberships { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=:memory;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().HasKey(e => e.UserName);
        modelBuilder.Entity<ApplicationUser>().HasMany(e => e.Groups)
            .WithMany(e => e.Members).UsingEntity<Membership>();
        modelBuilder.Entity<ApplicationUser>().HasMany(e => e.UserMessagesSent)
            .WithOne(e => e.Sender).HasForeignKey(e => e.SenderId);
        modelBuilder.Entity<ApplicationUser>().HasMany(e => e.UserMessagesReceived)
            .WithOne(e => e.Receiver).HasForeignKey(e => e.ReceiverId);
        modelBuilder.Entity<ApplicationUser>().HasMany(e => e.GroupMessagesSent)
            .WithOne(e => e.Sender).HasForeignKey(e => e.SenderId);
        base.OnModelCreating(modelBuilder);
    }
}