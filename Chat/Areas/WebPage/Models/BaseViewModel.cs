namespace Chat.Areas.WebPage.Models;
using Chat.Common.DTOs;
public class BaseViewModel
{
    public UserDTO? CurrentUser {get ; set;} = null;
}
