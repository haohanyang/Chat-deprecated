using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Chat.Domain;

namespace Chat.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserChannel> UserChannels { get; set; } = null!;
    public DbSet<GroupChannel> GroupChannels { get; set; } = null!;
    public DbSet<GroupMessage> GroupMessages { get; set; } = null!;
    public DbSet<UserMessage> UserMessages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Read database configs from environment variables 
        var server = Environment.GetEnvironmentVariable("DEV_DB_SERVER");
        var username = Environment.GetEnvironmentVariable("DEV_DB_USERNAME");
        var password = Environment.GetEnvironmentVariable("DEV_DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DEV_DB_DATABASE");
        if (server == null || username == null || password == null || database == null)
            throw new Exception("Environment variables of databases are not set");

        var connectionString =
            $"Server={server};User Id={username};Password={password};Database={database};TrustServerCertificate=true;";
        options.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(e => e.UserName).IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(e => e.GroupChannelMemberships)
            .WithOne(e => e.Member)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.MemberId)
            .IsRequired();

        modelBuilder.Entity<User>()
        .HasMany(e => e.UserChannels1)
        .WithOne(e => e.User1)
        .OnDelete(DeleteBehavior.NoAction)
        .HasForeignKey(e => e.User1Id)
        .IsRequired();

        modelBuilder.Entity<User>()
        .HasMany(e => e.UserChannels2)
        .WithOne(e => e.User2)
        .OnDelete(DeleteBehavior.NoAction)
        .HasForeignKey(e => e.User2Id)
        .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.CreatedGroupChannels)
            .WithOne(e => e.Creator)
            .HasForeignKey(e => e.CreatorId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        modelBuilder.Entity<GroupChannel>()
            .HasMany(e => e.Messages)
            .WithOne(e => e.Channel)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.ChannelId)
            .IsRequired();

        modelBuilder.Entity<GroupChannel>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.Channel)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.ChannelId)
            .IsRequired();

        modelBuilder.Entity<UserChannel>()
        .HasMany(e => e.Messages)
        .WithOne(e => e.Channel)
        .OnDelete(DeleteBehavior.NoAction)
        .HasForeignKey(e => e.ChannelId)
        .IsRequired();
    }
}