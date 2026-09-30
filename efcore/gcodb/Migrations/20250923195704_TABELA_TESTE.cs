using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gcodb.Migrations
{
    /// <inheritdoc />
    public partial class TABELA_TESTE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TABLE TABELATESTE( ID INT PRIMARY KEY, DESCRICAO VARCHAR(100), DATAHORA DATETIME)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE TABELATESTE");
        }
    }
}