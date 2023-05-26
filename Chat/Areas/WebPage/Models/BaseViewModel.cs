namespace Chat.Areas.WebPage.Models;
using Chat.Common.DTOs;
public class BaseViewModel
{
    public UserDto? CurrentUser {get ; set;} = null;
}
