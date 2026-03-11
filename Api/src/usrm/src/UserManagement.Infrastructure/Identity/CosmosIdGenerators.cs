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
    public override bool GeneratesTemporaryValues => false;

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
    public override bool GeneratesTemporaryValues => false;

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
    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
    {
        var userToken = (IdentityUserToken<Guid>)entry.Entity;
        return $"{userToken.UserId}|{userToken.LoginProvider}|{userToken.Name}";
    }
}
