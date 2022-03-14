﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Database;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.OAuth;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Controllers.OAuth;


[ApiController]
[Route("/oauth")]
public class TokensController : Controller
{

    
    private ITokensService _tokensService;
    private LazerContext _context;
    private IUserStorage _storage;

    public TokensController(ITokensService tokensService, LazerContext context, IUserStorage storage)
    {
        _tokensService = tokensService;
        _context = context;
        _storage = storage;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok();
    }
    [HttpPost("token")]
    public async Task<IActionResult> PostToken([FromForm] OauthTokenRequest body)
    {

    

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower().Replace(" ", "_") == body.Username.ToLower().Replace(" ", "_") ||
            u.Email == body.Username);

        if (user is null)
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: User not found.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }
        
        
        if (user.Banned)
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: User banned.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }

        if (!BCrypt.Net.BCrypt.Verify(body.Password, user.Password))
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: Invalid password.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }


        var token = _tokensService.GenerateToken();
        _storage.Users.Add(token.AccessToken, user);
        return Json(token);
    }
    
}