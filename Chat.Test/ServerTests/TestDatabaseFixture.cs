using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Test.ServerTests;

public class TestDatabaseFixture
{
    private static readonly object Lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (Lock)
        {
            if (!_databaseInitialized)
                using (var context = CreateDbContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    var users = new List<User>
                    {
                        new() { UserName = "user1" },
                        new() { UserName = "user2" },
                        new() { UserName = "user3" }
                    };
                    var groups = new List<Group>
                    {
                        new() { GroupName = "group1" },
                        new() { GroupName = "group2" },
                        new() { GroupName = "group3" }
                    };

                    // User1 and user2 in group1
                    var memberships = new List<Membership>
                    {
                        new() { User = users[0], Group = groups[0] },
                        new() { User = users[1], Group = groups[0] }
                    };
                    context.AddRange(users);
                    context.AddRange(groups);
                    context.AddRange(memberships);
                    context.SaveChanges();
                }

            _databaseInitialized = true;
        }
    }

    public ApplicationDbContext CreateDbContext()
    {
        var server = Environment.GetEnvironmentVariable("DEV_DATABASE_SERVER");
        var username = Environment.GetEnvironmentVariable("DEV_DATABASE_USERNAME");
        var password = Environment.GetEnvironmentVariable("DEV_DATABASE_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DEV_DATABASE_DATABASE");
        if (server == null || username == null || password == null || database == null)
            throw new Exception("Environment variables of databases are not set");

        var connectionString =
            $"Server={server};User Id={username};Password={password};Database={database};TrustServerCertificate=true;";
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options
        );
    }
}