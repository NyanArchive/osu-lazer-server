using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizationAttribute : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        var storage = context.HttpContext.RequestServices.GetRequiredService<IUserStorage>();
        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization") || !storage.Users.ContainsKey(context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")))
        {
            context.HttpContext.Response.StatusCode = 401;
            context.HttpContext.Response.WriteAsJsonAsync(new {error = "User not logged in."});
            context.HttpContext.Response.CompleteAsync();
            return Task.CompletedTask;
        }
        return next();
    }
}