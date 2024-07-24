using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // code trên dòng này thực hiện điều gì đó trước khi hành động (hàm) xảy ra
            var resultContext = await next();
            // code bên dưới thực hiện điều gì đó sau khi hành động (hàm) xảy ra

            if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

            var userId = resultContext.HttpContext.User.GetUserId();

            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            var user = await repo.GetUserByIdAsync(userId);

            if (user == null) return;
            user.LastActive = DateTime.Now;
            await repo.SaveAllAsync();
        }
    }
}
