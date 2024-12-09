using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace YourNamespace.Migrations
{
    public partial class AddResetPasswordFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Taikhoans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "Taikhoans",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Taikhoans");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "Taikhoans");
        }
    }
} 