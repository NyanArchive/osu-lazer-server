using Microsoft.AspNetCore.Mvc;

namespace OsuLazerServer.Models;

public class RegistrationBody
{
    [BindProperty(Name = "user[username]")]
    public string Username { get; set; }
    
    [BindProperty(Name = "user[user_email]")]
    public string Email { get; set; }

    [BindProperty(Name = "user[password]")]
    public string Password { get; set; }
}