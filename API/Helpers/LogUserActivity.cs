using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //tasks happening before action is executed

        var resultContext = await next();
        
        //tasks happening after action is executed
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
        var userID = resultContext.HttpContext.User.GetUserId();
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByIdAsync(userID);

        if (user == null) return;

        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync(); 
    }
}