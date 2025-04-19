using System.Security.Claims;
using System.Text.Json;
using AspireLearning.Api.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.Api.Services;

public class RoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly SessionModel _session;

    public RoleService(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        SessionModel session)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _session = session;
    }

    // Rol oluşturma
    public async Task<IdentityResult> CreateRoleAsync(RoleCreateModel model)
    {
        var tenantId = _session.TenantId;
        
        // Rol adının benzersiz olduğunu kontrol et (tenant içinde)
        var existingRole = await _roleManager.Roles
            .Where(r => r.TenantId == tenantId && r.Name == model.Name)
            .FirstOrDefaultAsync();
        
        if (existingRole != null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = "Bu rol adı zaten kullanılıyor."
            });
        }

        var role = new Role(model.Name)
        {
            TenantId = tenantId
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return result;
        }

        // Rol oluşturuldu, şimdi izinleri ekleyelim
        foreach (var permission in model.Permissions)
        {
            await AddPermissionToRoleAsync(role, permission);
        }

        return IdentityResult.Success;
    }

    // Rol güncelleme
    public async Task<IdentityResult> UpdateRoleAsync(Guid roleId, RoleUpdateModel model)
    {
        var tenantId = _session.TenantId;
        
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
        
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Rol bulunamadı."
            });
        }

        // Rolün adını güncelle
        role.Name = model.Name;
        role.NormalizedName = model.Name.ToUpper();

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return result;
        }

        // Mevcut tüm izinleri temizle
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Yeni izinleri ekle
        foreach (var permission in model.Permissions)
        {
            await AddPermissionToRoleAsync(role, permission);
        }

        return IdentityResult.Success;
    }

    // Rol silme
    public async Task<IdentityResult> DeleteRoleAsync(Guid roleId)
    {
        var tenantId = _session.TenantId;
        
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
        
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Rol bulunamadı."
            });
        }

        return await _roleManager.DeleteAsync(role);
    }

    // Tenant için rolleri listeleme
    public async Task<List<RoleModel>> GetRolesAsync()
    {
        var tenantId = _session.TenantId;
        
        var roles = await _roleManager.Roles
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        var result = new List<RoleModel>();
        
        foreach (var role in roles)
        {
            var roleModel = new RoleModel
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = []
            };
            
            var claims = await _roleManager.GetClaimsAsync(role);
            roleModel.Permissions = claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToArray();
            
            result.Add(roleModel);
        }

        return result;
    }

    // Belirli bir rolü getirme
    public async Task<RoleModel?> GetRoleByIdAsync(Guid roleId)
    {
        var tenantId = _session.TenantId;
        
        var role = await _roleManager.Roles
            .Where(r => r.Id == roleId && r.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (role == null)
        {
            return null;
        }
        
        var roleModel = new RoleModel
        {
            Id = role.Id,
            Name = role.Name,
            Permissions = []
        };
        
        var claims = await _roleManager.GetClaimsAsync(role);
        roleModel.Permissions = claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToArray();
        
        return roleModel;
    }

    // Kullanıcıya rol atama
    public async Task<IdentityResult> AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        var tenantId = _session.TenantId;
        
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
        
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "Kullanıcı bulunamadı."
            });
        }

        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
        
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Rol bulunamadı."
            });
        }

        return await _userManager.AddToRoleAsync(user, role.Name);
    }

    // Kullanıcıdan rol kaldırma
    public async Task<IdentityResult> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var tenantId = _session.TenantId;
        
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
        
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "Kullanıcı bulunamadı."
            });
        }

        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
        
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Rol bulunamadı."
            });
        }

        return await _userManager.RemoveFromRoleAsync(user, role.Name);
    }

    // Role izin ekleme (yardımcı metot)
    private async Task<IdentityResult> AddPermissionToRoleAsync(Role role, string permission)
    {
        var claim = new Claim("Permission", permission);
        return await _roleManager.AddClaimAsync(role, claim);
    }

    // Kullanıcının tüm izinlerini getirme
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var tenantId = _session.TenantId;
        
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
        
        if (user == null)
        {
            return new List<string>();
        }

        // Kullanıcının rollerini al
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new HashSet<string>();

        // Her rolün izinlerini getir
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims.Where(c => c.Type == "Permission"))
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        return permissions.ToList();
    }

    // Tüm kullanılabilir izinleri getirme
    public List<string> GetAllPermissions()
    {
        return Permissions.AllPermissions;
    }
} 