using API.Data;
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
        var unitOfWork = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var user = await unitOfWork.UserRepository.GetUserByIdAsync(userID);

        if (user == null) return;

        user.LastActive = DateTime.UtcNow;
        await unitOfWork.Complete(); 
    }
}