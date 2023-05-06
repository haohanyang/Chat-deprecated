using Chat.Common;
using Chat.Areas.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;


    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("all")]
    public async IAsyncEnumerable<UserDTO> AllUsers()
    {
        var users = await _userService.GetAllUsers();
        foreach (var user in users)
        {
            yield return user;
        }
    }

}