using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Generates a unique id for IdentityUserRole by combining UserId and RoleId
/// Required for Cosmos DB which needs a single string id property
/// </summary>
internal sealed class UserRoleIdGenerator : ValueGenerator<string>
{
    /// <summary>
    /// Gets a value indicating whether generated values are temporary; always false.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Builds the id as the user id and role id separated by a pipe.
    /// </summary>
    public override string Next(EntityEntry entry)
    {
        var userRole = (IdentityUserRole<Guid>)entry.Entity;
        return $"{userRole.UserId}|{userRole.RoleId}";
    }
}

/// <summary>
/// Generates a unique id for IdentityUserLogin by combining LoginProvider and ProviderKey
/// Required for Cosmos DB which needs a single string id property
/// </summary>
internal sealed class UserLoginIdGenerator : ValueGenerator<string>
{
    /// <summary>
    /// Gets a value indicating whether generated values are temporary; always false.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Builds the id as the login provider and provider key separated by a pipe.
    /// </summary>
    public override string Next(EntityEntry entry)
    {
        var userLogin = (IdentityUserLogin<Guid>)entry.Entity;
        return $"{userLogin.LoginProvider}|{userLogin.ProviderKey}";
    }
}

/// <summary>
/// Generates a unique id for IdentityUserToken by combining UserId, LoginProvider, and Name
/// Required for Cosmos DB which needs a single string id property
/// </summary>
internal sealed class UserTokenIdGenerator : ValueGenerator<string>
{
    /// <summary>
    /// Gets a value indicating whether generated values are temporary; always false.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Builds the id as the user id, login provider and token name separated by pipes.
    /// </summary>
    public override string Next(EntityEntry entry)
    {
        var userToken = (IdentityUserToken<Guid>)entry.Entity;
        return $"{userToken.UserId}|{userToken.LoginProvider}|{userToken.Name}";
    }
}
