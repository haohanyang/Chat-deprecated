namespace Chat.Areas.WebPage.Models;
using Chat.Common.DTOs;
public class BaseViewModel
{
    public User? LoggedInUser {get ; set;} = null;
}
