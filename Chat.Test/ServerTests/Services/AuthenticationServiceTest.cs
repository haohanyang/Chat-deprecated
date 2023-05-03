using System.Security.Authentication;
using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chat.Test.ServerTests.Services;

public class AuthenticationTokenServiceTest
{
    private readonly List<User> _users = new()
    {
        new() { UserName = "user1" },
        new() { UserName = "user2" }
    };

    private static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> list) where TUser : User
    {
        var store = new Mock<IUserStore<TUser>>();
        var mockUserManager = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mockUserManager.Object.UserValidators.Add(new UserValidator<TUser>());
        mockUserManager.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string username) => list.Find(e => e.UserName == username));
        mockUserManager.Setup(m => m.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync((TUser user, string password) => list.Find(e => e.UserName == user.UserName) == null ?
                IdentityResult.Success : IdentityResult.Failed())
            .Callback<TUser, string>((x, y) => list.Add(x));

        return mockUserManager;
    }

    private static UserManager<TUser> _MockUserManager<TUser>(List<TUser> list) where TUser : User
    {
        var store = new Mock<IUserPasswordStore<TUser>>();
        store.Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string username, CancellationToken token) => list.Find(e => e.UserName == username));
        store.Setup(s => s.CreateAsync(It.IsAny<TUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TUser user, CancellationToken token) => list.Find(e => e.UserName == user.UserName) != null ? 
                IdentityResult.Failed() : IdentityResult.Success)
            .Callback<TUser, CancellationToken>((user, token) =>
            {
                if (!list.Select(e => e.UserName).Contains(user.UserName))
                {
                    list.Add(user);
                }
            });
        
        var options = new Mock<IOptions<IdentityOptions>>();
        
        var identityOptions = new IdentityOptions();
        identityOptions.Password.RequireNonAlphanumeric = false;
        identityOptions.Password.RequiredLength = 4;
        identityOptions.Password.RequireUppercase = false;
        identityOptions.Password.RequireLowercase = false;
        identityOptions.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
        
        options.Setup(o => o.Value).Returns(identityOptions);
        
        var userValidators = new List<IUserValidator<TUser>>() {new UserValidator<TUser>()};
        var pwdValidators = new List<IPasswordValidator<TUser>>() {new PasswordValidator<TUser>()};
        
        
        var userManager = new UserManager<TUser>(store.Object, options.Object, new PasswordHasher<TUser>(),
            userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), null,
            new Mock<ILogger<UserManager<TUser>>>().Object);
        
        return userManager;
    }

    [Fact]
    public async Task TestRegister()
    {
        var userManager = MockUserManager(_users).Object;
        var authenticationService =
            new AuthenticationService(new LoggerFactory().CreateLogger<AuthenticationService>(), userManager);
        var result1 = await authenticationService.Register("user3", "password");
        Assert.True(result1.Succeeded);
        
        await Assert.ThrowsAsync<ArgumentException>(async () => await authenticationService.Register("user1", "password"));
    }

    [Fact]
    public async Task TestLogin()
    {
        var userManager = MockUserManager(_users).Object;
        var authenticationService =
            new AuthenticationService(new LoggerFactory().CreateLogger<AuthenticationService>(), userManager);
        await authenticationService.Login("user1", "password");
        await Assert.ThrowsAsync<AuthenticationException>(async () => await authenticationService.Login("user1", "wrong_password"));
    }
    
}