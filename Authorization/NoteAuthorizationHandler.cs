using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NoteManagementAPI.Models;
using System.Security.Claims;

namespace NoteManagementAPI.Authorization
{
    public sealed class NoteAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, Note>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Note resource)
        {
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != resource.OwnerUserId)
            {
                return Task.CompletedTask;
            }

            if (requirement.Name == NoteOperations.Read.Name ||
                requirement.Name == NoteOperations.Update.Name ||
                requirement.Name == NoteOperations.Delete.Name)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
