using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gcodb.Migrations
{
    /// <inheritdoc />
    public partial class INS_REGISTROS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO TABELATESTE (ID, DESCRICAO, DATAHORA) VALUES (1, 'Registro1', GETDATE())");
            migrationBuilder.Sql("INSERT INTO TABELATESTE (ID, DESCRICAO, DATAHORA) VALUES (2, 'Registro2', GETDATE())");
            migrationBuilder.Sql("INSERT INTO TABELATESTE (ID, DESCRICAO, DATAHORA) VALUES (3, 'Registro3', GETDATE())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM TABELATESTE WHERE ID IN (1, 2, 3)");
        }
    }
}