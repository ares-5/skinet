using System.Security.Authentication;
using System.Security.Claims;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static async Task<AppUser> GetUserByEmailAsync(this UserManager<AppUser> userManager, ClaimsPrincipal user)
    {
        var userToReturn = await userManager.Users
            .FirstOrDefaultAsync(x => x.Email.Equals(user.GetEmail()));

        if (userToReturn is null)
        {
            throw new AuthenticationException("User not found");
        }

        return userToReturn;
    }
    
    public static async Task<AppUser> GetUserByEmailWithAddressAsync(this UserManager<AppUser> userManager, ClaimsPrincipal user)
    {
        var userToReturn = await userManager.Users
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.Email.Equals(user.GetEmail()));

        if (userToReturn is null)
        {
            throw new AuthenticationException("User not found");
        }

        return userToReturn;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        return email ?? throw new AuthenticationException("Email claim not found");
    }
}