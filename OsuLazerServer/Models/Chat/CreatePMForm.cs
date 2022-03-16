using Microsoft.AspNetCore.Mvc;

namespace OsuLazerServer.Models.Chat;

public class CreatePMForm
{
    public string Type { get; set; }
    
    [BindProperty(Name = "target_id")]
    public int TargetId { get; set; }
}