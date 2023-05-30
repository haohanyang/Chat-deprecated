namespace Chat.Areas.WebPage.Models;
using Chat.Common.Dto;
public class HomeViewModel : BaseViewModel
{
    public String? Error;
    public RedirectMessage? RedirectMessage { get; set; }
}