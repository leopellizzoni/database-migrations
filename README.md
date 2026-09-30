# database-migrations


O mesmo banco (tabela `TABELATESTE` com três registros) é mantido por **duas ferramentas de migration**, em duas trilhas independentes:

| Trilha | Pasta | Você escreve | Banco padrão | Recomendada para |
|---|---|---|---|---|
| **Flyway** | [`flyway/`](flyway/README.md) | arquivos `.sql` | SQLite (arquivo local, nada para instalar) | quem ainda não programa em C# ou está começando em banco de dados |
| **EF Core Migrations** | [`efcore/`](efcore/README.md) | classes C# (`dotnet ef migrations add`) | SQL Server | quem já usa .NET |

As duas trilhas fazem as mesmas práticas. Escolha **uma** e siga o README da pasta.

## O que é uma migration

Uma migration é uma mudança no banco (criar tabela, adicionar coluna, inserir dados) que é:

1. **versionada**: um arquivo no Git, revisado no Pull Request como qualquer código;
2. **ordenada**: a ferramenta sabe em que ordem aplicar;
3. **aplicada uma única vez** por banco;
4. **registrada no próprio banco**, numa tabela de histórico:

| Ferramenta | Tabela de histórico |
|---|---|
| Flyway | `flyway_schema_history` |
| EF Core | `__EFMigrationsHistory` |

Como a ferramenta consulta essa tabela antes de executar, **os scripts não precisam de `IF NOT EXISTS`**. Cada migration roda uma vez e fica registrada.

**Regra de ouro:** nunca edite uma migration que já foi aplicada em um banco compartilhado. Para corrigir, crie uma migration nova.

## Estrutura

```text
.
├── README.md                  ← você está aqui
├── docs/
│   └── praticas.md            ← roteiros das Práticas 1, 2 e 3 (as duas trilhas)
├── flyway/
│   ├── README.md
│   ├── flyway.toml            ← configuração (SQLite por padrão)
│   └── sql/                   ← migrations em SQL
│       ├── V1__cria_tabelateste.sql
│       ├── V2__insere_registros.sql
│       └── R__vw_tabelateste.sql
├── efcore/
│   ├── README.md
│   ├── gcodb.sln
│   ├── .config/dotnet-tools.json   ← versão fixa do dotnet-ef
│   └── gcodb/
│       ├── ContextoBancoDados.cs
│       ├── appsettings.json               ← modelo, sem senha real
│       ├── appsettings.Local.exemplo.json ← copie para appsettings.Local.json
│       └── Migrations/
└── .github/workflows/migrations.yml   ← CI: valida as duas trilhas a cada push/PR
```

## Como começar

1. Faça um **fork** deste repositório para a sua conta do GitHub.
2. Clone o seu fork:
   ```bash
   git clone https://github.com/<seu-usuario>/database-migrations.git
   cd database-migrations
   ```
3. Crie as branches de integração usadas nas práticas (o repositório original só tem `master`):
   ```bash
   git switch -c develop && git push -u origin develop
   git switch -c release && git push -u origin release
   git switch develop
   ```
4. Siga o README da trilha escolhida e depois o roteiro em [`docs/praticas.md`](docs/praticas.md).

## Referências

- Flyway: https://documentation.red-gate.com/fd
- EF Core Migrations: https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- SADALAGE, P.; FOWLER, M. *Evolutionary Database Design*. https://martinfowler.com/articles/evodb.html
- SATO, D. *ParallelChange* (expand/contract). https://martinfowler.com/bliki/ParallelChange.html
