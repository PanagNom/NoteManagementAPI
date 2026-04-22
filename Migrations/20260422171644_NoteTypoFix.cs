using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class NoteTypoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifieddBy",
                table: "Notes",
                newName: "ModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "Notes",
                newName: "ModifieddBy");
        }
    }
}
