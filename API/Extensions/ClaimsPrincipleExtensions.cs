﻿using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // không nên dùng exception dùng ở đây là vì username ko thể null đc
            var username = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get username from token");
            return username;
        }
    }
}
