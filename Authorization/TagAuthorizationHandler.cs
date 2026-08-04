using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NoteManagementAPI.Models;
using System.Security.Claims;

namespace NoteManagementAPI.Authorization
{
    public sealed class TagAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, Tag>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Tag resource)
        {
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != resource.OwnerUserId)
            {
                return Task.CompletedTask;
            }

            if (requirement.Name == TagOperations.Read.Name ||
                requirement.Name == TagOperations.Update.Name ||
                requirement.Name == TagOperations.Delete.Name)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
