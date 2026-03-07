using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalagoApi.Migrations
{
    /// <inheritdoc />
    public partial class IndiceProdutoNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Nome",
                table: "Produtos",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Produtos_Nome",
                table: "Produtos");
        }
    }
}
