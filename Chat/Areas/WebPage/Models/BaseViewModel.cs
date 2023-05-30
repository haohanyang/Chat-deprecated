namespace Chat.Areas.WebPage.Models;
using Chat.Common.Dto;
public class BaseViewModel
{
    public UserDto? CurrentUser {get ; set;} = null;
}
