# Trilha EF Core Migrations

Projeto .NET (`gcodb`) com migrations do Entity Framework Core. Banco padrão: **SQL Server**.

Neste repositório as migrations não usam entidades: cada uma executa SQL diretamente com `migrationBuilder.Sql(...)` (veja `Migrations/20250923195704_TABELA_TESTE.cs`). Assim, a trilha EF Core cria exatamente o mesmo banco que a trilha Flyway.

## Pontos principais

| Item | Onde |
|---|---|
| Conexão com o banco | `gcodb/ContextoBancoDados.cs` |
| String de conexão | `gcodb/appsettings.Local.json` (seu, fora do Git) ou variável de ambiente |
| Migrations | `gcodb/Migrations/` |
| Tabela de histórico no banco | `__EFMigrationsHistory` |
| Versão da ferramenta `dotnet-ef` | `.config/dotnet-tools.json` (fixada em 8.0.10) |

## 1. Configurar a conexão (uma vez)

1. Copie `gcodb/appsettings.Local.exemplo.json` para `gcodb/appsettings.Local.json`.
2. Edite a string de conexão com o seu servidor, usuário e senha.

O `appsettings.Local.json` está no `.gitignore`, então a sua senha não vai para o repositório. O `appsettings.json` versionado é só o modelo.

A variável de ambiente `ConnectionStrings__DefaultConnection`, se existir, tem prioridade sobre os arquivos (é assim que o CI configura a conexão).

Para usar PostgreSQL, troque `UseSqlServer` por `UseNpgsql` em `ContextoBancoDados.cs`. A migration `INS_REGISTROS` usa `GETDATE()`, que só existe no SQL Server; em PostgreSQL ela falha. É um exemplo de migration presa a um SGBD.

## 2. Via dotnet CLI ou terminal do VS Code

Rode **de dentro da pasta `efcore/gcodb/`**:

```bash
cd efcore/gcodb
dotnet tool restore                                 # instala o dotnet-ef na versão do repositório (uma vez)
dotnet ef migrations list                           # migrations do projeto e se já foram aplicadas
dotnet ef migrations add NomeDaMigracao             # cria uma migration
dotnet ef database update                           # aplica as pendentes
dotnet ef database update NOME_MIGRACAO_DESTINO     # volta (downgrade) até a migration destino
dotnet ef migrations script --idempotent -o migrate.sql   # gera o SQL para revisão
dotnet ef migrations has-pending-model-changes      # falha se o modelo mudou sem migration
```

Se preferir a ferramenta global: `dotnet tool install --global dotnet-ef --version 8.0.10`.

## 3. Via Visual Studio (Package Manager Console)

*View → Other Windows → Package Manager Console*. Selecione o projeto `gcodb` em *Default project*.

| Comando | Equivalente na CLI |
|---|---|
| `Add-Migration NomeDaMigracao` | `dotnet ef migrations add` |
| `Update-Database` | `dotnet ef database update` |
| `Update-Database NOME_MIGRACAO_DESTINO` | downgrade |
| `Script-Migration -Idempotent` | `dotnet ef migrations script --idempotent` |

## 4. Migration com SQL direto

Depois de `dotnet ef migrations add NomeDaMigracao`, edite o arquivo gerado:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql("ALTER TABLE TABELATESTE ADD OBSERVACAO VARCHAR(200) NULL");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql("ALTER TABLE TABELATESTE DROP COLUMN OBSERVACAO");
}
```

`Up` aplica a mudança; `Down` desfaz (usado no downgrade).

## 5. Onde o EF Core registra o que foi executado

```sql
SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory;
```

O EF Core guarda **só o id e a versão do EF**, sem checksum. Se alguém editar uma migration já aplicada, o EF não percebe. O Flyway percebe; compare na Prática 1.

## Nota sobre a versão do .NET

O projeto usa **.NET 8**, cujo suporte termina em **10/11/2026**. Atualizar para o .NET 10 (LTS) é um bom exercício da aula 10A: mude o `TargetFramework`, os pacotes `Microsoft.EntityFrameworkCore.*` e a versão do `dotnet-ef` no `.config/dotnet-tools.json`.
