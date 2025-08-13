using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFamilyTreeNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "FamilyMembers");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "FamilyMembers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Families",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Families");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "FamilyMembers",
                type: "TEXT",
                nullable: true);
        }
    }
}
