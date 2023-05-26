using Chat.Areas.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Areas.Api.Data;


public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Membership> Memberships { get; set; } = null!;
    public DbSet<GroupMessage> GroupMessages { get; set; } = null!;
    public DbSet<UserMessage> UserMessages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Read database configs from environment variables 
        var server = Environment.GetEnvironmentVariable("DEV_DATABASE_SERVER");
        var username = Environment.GetEnvironmentVariable("DEV_DATABASE_USERNAME");
        var password = Environment.GetEnvironmentVariable("DEV_DATABASE_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DEV_DATABASE_DATABASE");
        if (server == null || username == null || password == null || database == null)
            throw new Exception("Environment variables of databases are not set");

        var connectionString =
            $"Server={server};User Id={username};Password={password};Database={database};TrustServerCertificate=true;";
        options.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User-group relationships
        modelBuilder.Entity<User>()
            .HasIndex(e => e.UserName).IsUnique();
        modelBuilder.Entity<User>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.User)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.UserId)
            .IsRequired();
        modelBuilder.Entity<Group>()
            .HasIndex(e => e.Name)
            .IsUnique();
        modelBuilder.Entity<Group>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.Group)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.GroupId)
            .IsRequired();
        modelBuilder.Entity<Group>().
            HasOne(e => e.Creator)
            .WithMany(e => e.CreatedGroups)
            .HasForeignKey(e => e.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);

        // User-message relationships
        modelBuilder.Entity<User>()
            .HasMany(e => e.UserMessagesSent)
            .WithOne(e => e.Sender)
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.UserMessagesReceived)
            .WithOne(e => e.Receiver)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.GroupMessagesSent)
            .WithOne(e => e.Sender)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.SenderId)
            .IsRequired();

        // Group-message relationships
        modelBuilder.Entity<Group>()
            .HasMany(e => e.Messages)
            .WithOne(e => e.Receiver)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.ReceiverId);
    }
}