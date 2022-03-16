using Microsoft.AspNetCore.Mvc;

namespace OsuLazerServer.Models.Chat;

public class MessagePostBody
{
    public bool IsAction { get; set; }
    public string Message { get; set; }
    
    
    [BindProperty(Name = "target_id")]
    public int? TargetId { get; set; }
}