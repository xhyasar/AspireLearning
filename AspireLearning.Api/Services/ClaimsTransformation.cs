using System.Security.Claims;
using AspireLearning.Api.Data.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Api.Services;

public class ClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public ClaimsTransformation(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return principal;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return principal;
        }

        var userId = userIdClaim.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return principal;
        }

        // Kullanıcının rollerini al
        var userRoles = await _userManager.GetRolesAsync(user);
        
        // SuperAdmin rolü varsa, doğrudan tüm izinleri ekle
        if (userRoles.Contains(RoleConstants.SuperAdmin.Name))
        {
            // SuperAdmin tüm izinlere sahiptir, izinleri kontrol etmeye gerek yok
            return principal;
        }

        // Rol bazlı izinleri ekle
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                // Bu rolün tüm izinlerini al
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                {
                    // İzin claim'i zaten mevcut değilse ekle
                    if (!principal.HasClaim(c => c.Type == "Permission" && c.Value == claim.Value))
                    {
                        // ClaimsIdentity'e çalışma zamanında claim ekle
                        identity.AddClaim(new Claim("Permission", claim.Value));
                    }
                }
            }
        }

        return principal;
    }
} 