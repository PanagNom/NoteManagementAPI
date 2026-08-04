using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace NoteManagementAPI.Authorization
{
    public static class NoteOperations
    {
        public static OperationAuthorizationRequirement Read { get; } = new() { Name = nameof(Read) };
        public static OperationAuthorizationRequirement Update { get; } = new() { Name = nameof(Update) };
        public static OperationAuthorizationRequirement Delete { get; } = new() { Name = nameof(Delete) };
    }
}
