using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTagOwnership : Migration
    {
        private const string LegacyOwnerId = "7b585d88-0ff4-4b74-a51d-f33fe3b7a6c4";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Tags",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql($"""
                IF EXISTS (SELECT 1 FROM [Tags])
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

                UPDATE [Tags]
                SET [OwnerUserId] = N'{LegacyOwnerId}'
                WHERE [OwnerUserId] IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Tags",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerUserId_Name",
                table: "Tags",
                columns: new[] { "OwnerUserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AspNetUsers_OwnerUserId",
                table: "Tags",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AspNetUsers_OwnerUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_OwnerUserId_Name",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.Sql($"""
                DELETE FROM [AspNetUsers]
                WHERE [Id] = N'{LegacyOwnerId}'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [Notes]
                      WHERE [OwnerUserId] = N'{LegacyOwnerId}');
                """);
        }
    }
}
