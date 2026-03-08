using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Tests.Helpers;

/// <summary>
/// Helper class for creating mocks of ASP.NET Core Identity components
/// </summary>
public static class MockHelpers
{
    public static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);

        return userManager;
    }

    public static Mock<SignInManager<ApplicationUser>> MockSignInManager(Mock<UserManager<ApplicationUser>> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();

        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            contextAccessor.Object,
            userPrincipalFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            confirmation.Object);

        return signInManager;
    }

    public static Mock<RoleManager<IdentityRole<Guid>>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleValidators = new List<IRoleValidator<IdentityRole<Guid>>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<IdentityRole<Guid>>>>();

        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            store.Object,
            roleValidators,
            keyNormalizer.Object,
            errors.Object,
            logger.Object);

        return roleManager;
    }
}
