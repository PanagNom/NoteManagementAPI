using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteOwnership : Migration
    {
        private const string LegacyOwnerId = "7b585d88-0ff4-4b74-a51d-f33fe3b7a6c4";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Notes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql($"""
                IF EXISTS (SELECT 1 FROM [Notes])
                   AND NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'{LegacyOwnerId}')
                BEGIN
                    INSERT INTO [AspNetUsers]
                        ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName],
                         [EmailConfirmed], [PhoneNumberConfirmed], [TwoFactorEnabled],
                         [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
                    VALUES
                        (N'{LegacyOwnerId}', N'Legacy', N'Notes',
                         N'__legacy_notes_owner_7b585d88__', N'__LEGACY_NOTES_OWNER_7B585D88__',
                         0, 0, 0, '9999-12-31T23:59:59.9999999+00:00', 1, 0);
                END

                UPDATE [Notes]
                SET [OwnerUserId] = N'{LegacyOwnerId}'
                WHERE [OwnerUserId] IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Notes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_OwnerUserId",
                table: "Notes",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AspNetUsers_OwnerUserId",
                table: "Notes",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AspNetUsers_OwnerUserId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_OwnerUserId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Notes");

            migrationBuilder.Sql($"""
                DELETE FROM [AspNetUsers]
                WHERE [Id] = N'{LegacyOwnerId}';
                """);
        }
    }
}
