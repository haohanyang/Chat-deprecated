using System.Linq.Expressions;
using Chat.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Chat.Common.Dto;
using System.Security.Claims;

namespace Chat.Areas.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly IUserChannelService _userChannelService;
    private readonly IGroupChannelService _groupChannelService;

    public UserController(IUserService userService, IGroupChannelService groupChannelService, IUserChannelService userChannelService, ILogger<UserController> logger)
    {
        _userService = userService;
        _groupChannelService = groupChannelService;
        _userChannelService = userChannelService;
        _logger = logger;
    }

    /// <summary>
    /// Get the user's profile
    /// </summary>
    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute(Name = "username")] string username)
    {
        return await _userService.GetUser(username);
    }
}