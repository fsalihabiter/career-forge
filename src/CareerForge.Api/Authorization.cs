using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api;

public static class AppRoles
{
    public const string Student = "Student";
    public const string Administrator = "Administrator";
}

public static class AppPolicies
{
    public const string StudentAccess = "StudentAccess";
    public const string AdministratorAccess = "AdministratorAccess";
}

public static class RoleSeed
{
    public static async Task ApplyAsync(
        RoleManager<IdentityRole<Guid>> roles,
        UserManager<Models.AppUser> users)
    {
        foreach (var roleName in new[] { AppRoles.Student, AppRoles.Administrator })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var result = await roles.CreateAsync(new IdentityRole<Guid>(roleName));
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"'{roleName}' rolü oluşturulamadı: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }

        var existingUsers = await users.Users.ToListAsync();
        foreach (var user in existingUsers)
        {
            if ((await users.GetRolesAsync(user)).Count == 0)
            {
                var result = await users.AddToRoleAsync(user, AppRoles.Student);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"'{user.Email}' kullanıcısına öğrenci rolü atanamadı: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }
    }
}
