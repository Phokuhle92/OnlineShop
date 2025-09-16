namespace OnlineShop.API.Security
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;
    using System;

    public class OtpAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string? _role;

        public OtpAuthorizeAttribute(string? role = null)
        {
            _role = role;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new JsonResult(new { message = "User not authenticated." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return Task.CompletedTask;
            }

            // OTP session check
            var sessionUserId = httpContext.Session.GetString("OtpVerifiedUserId");
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(sessionUserId) || userId != sessionUserId)
            {
                context.Result = new JsonResult(new { message = "OTP verification required." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return Task.CompletedTask;
            }

            // Role check
            if (!string.IsNullOrEmpty(_role))
            {
                var sessionRole = httpContext.Session.GetString("OtpVerifiedUserRole");

                bool isInRole = !string.IsNullOrEmpty(sessionRole)
                    ? sessionRole == _role
                    : user.IsInRole(_role);

                if (!isInRole)
                {
                    context.Result = new JsonResult(new { message = "Access denied. Required role: " + _role })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
