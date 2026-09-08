using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaOnline.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUsuarioCpf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Usuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 14,
                nullable: false,
                defaultValue: "");
        }
    }
}
