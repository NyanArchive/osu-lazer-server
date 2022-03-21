using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OsuLazerServer.Database;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiredLazerClient : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var token = context.HttpContext.Request.Headers["Authorization"].ToString()
            .Replace("Bearer ", "");
        var storage = context.HttpContext.RequestServices.GetRequiredService<IUserStorage>();
        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        var ctx = context.HttpContext.RequestServices.GetRequiredService<LazerContext>();
        
        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization") || !storage.Users.ContainsKey(context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")))
        {
            var cachedUserId = cache.Get($"token:{token}");
            //We trying to get it from redis.
            if (cachedUserId is not null)
            {
                var expiresAtRaw =  Convert.ToInt64(Encoding.UTF8.GetString(await cache.GetAsync($"token:{token}:expires_at") ?? Array.Empty<byte>()));

                if (expiresAtRaw <
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    context.HttpContext.Response.StatusCode = 401;
                    await context.HttpContext.Response.WriteAsJsonAsync(new {authentication = "basic"});
                    await context.HttpContext.Response.CompleteAsync(); 
                }

                var user = await ctx.Users.FirstAsync(u => u.Id == Convert.ToInt32(Encoding.UTF8.GetString(cachedUserId)));

                storage.Users.TryAdd(token, user);
                await next();
                return;
            }
            
            context.HttpContext.Response.StatusCode = 401;
            await context.HttpContext.Response.WriteAsJsonAsync(new {authentication = "basic"});
            await context.HttpContext.Response.CompleteAsync();
            return;
        }
        await next();
    }
}