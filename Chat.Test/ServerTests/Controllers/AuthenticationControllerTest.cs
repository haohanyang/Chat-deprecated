using Chat.Server.Controllers;
using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chat.Test.ServerTests.Controllers;

public class AuthenticationControllerTest
{
    [Fact]
    public async Task Test()
    {
        var mockUserManager = new Mock<UserManager<User>>();
        var authenticationService = new Mock<AuthenticationService>();
        var controller = new AuthenticationController(authenticationService.Object,new LoggerFactory().CreateLogger<AuthenticationController>());
    }
}