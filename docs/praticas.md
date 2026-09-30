# Práticas: migrations em branches paralelas

Três práticas em sequência, **no seu fork**, na trilha que você escolheu (Flyway ou EF Core). Cada prática traz os passos das duas trilhas e as perguntas que você deve responder.

Antes de começar:

- as branches `develop` e `release` existem no seu fork (veja o [README principal](../README.md#como-começar));
- o banco local está atualizado:
  - Flyway: `cd flyway && flyway -configFiles=flyway.toml migrate`
  - EF Core: `cd efcore/gcodb && dotnet ef database update`

Registre as respostas na descrição dos Pull Requests.

---

## Prática 1: uma migration do começo ao fim (≈ 10 min)

**Objetivo:** criar uma migration numa feature e verificar se ela chega igual a `develop` e `release`, e ver como cada ferramenta reage quando alguém edita uma migration já aplicada.

### Passos

```bash
git switch develop
git switch -c featTeste
```

| | Flyway | EF Core |
|---|---|---|
| Criar a migration | crie `flyway/sql/V3__adiciona_coluna_ativo.sql` com<br>`ALTER TABLE TABELATESTE ADD COLUMN ATIVO INT DEFAULT 1;` | `dotnet ef migrations add ADICIONA_COLUNA_ATIVO` e, no `Up`:<br>`migrationBuilder.Sql("ALTER TABLE TABELATESTE ADD ATIVO INT NULL");` |
| Aplicar | `flyway -configFiles=flyway.toml migrate` | `dotnet ef database update` |
| Conferir | `flyway -configFiles=flyway.toml info` | `dotnet ef migrations list` |

```bash
git add . && git commit -m "feat: coluna ATIVO"
git switch develop && git merge featTeste
git switch release && git merge develop
git diff featTeste release -- flyway/sql efcore/gcodb/Migrations   # deve sair vazio
```

**Parte 2: editar uma migration já aplicada**

1. Edite a migration que você acabou de aplicar (por exemplo, troque `DEFAULT 1` por `DEFAULT 0`).
2. Flyway: rode `flyway -configFiles=flyway.toml validate`. EF Core: rode `dotnet ef database update`.
3. Desfaça a edição (`git restore <arquivo>`).

### Responda

1. A migration está presente e idêntica em `featTeste`, `develop` e `release`?
2. Que linha nova apareceu na tabela de histórico (`flyway_schema_history` ou `__EFMigrationsHistory`)? Que informações ela guarda?
3. Na Parte 2, qual ferramenta percebeu a edição? Por que uma percebe e a outra não?

---

## Prática 2: três features integradas fora de ordem (≈ 20 min)

**Objetivo:** ver o que acontece quando migrations criadas em paralelo são integradas em ordem diferente da ordem em que foram criadas.

Crie as três branches **a partir do mesmo ponto de `develop`**, nesta ordem:

| Branch | Migrations (crie nesta ordem) |
|---|---|
| `featAlfa` | Alfa1: adiciona a coluna `COR VARCHAR(20)` · Alfa2: `INSERT INTO TABELATESTE (ID, DESCRICAO, DATAHORA, COR) VALUES (4, 'Registro4', CURRENT_TIMESTAMP, 'azul')` |
| `featBeta` | Beta1: adiciona a coluna `PESO INT` |
| `featGama` | Gama1: adiciona a coluna `CODIGO VARCHAR(10)` |

**Trilha Flyway:** cada branch usa o próximo número livre que ela enxerga. Como todas partem de `develop` com `V3`, as três criam `V4` (e `featAlfa` também `V5`).

Integre **nesta ordem**, com Pull Request e atualização do banco após cada merge:

1. PR `featBeta` → `develop`; merge; migrate / database update; observe a tabela de histórico.
2. PR `featGama` → `develop`; merge; migrate / database update; observe.
3. PR `featAlfa` → `develop`; merge; migrate / database update; observe.

### O que observar

| | Flyway | EF Core |
|---|---|---|
| Passo 2 | `Found more than one migration with version 4`: o migrate para. Renumere `Gama1` para `V5` e rode de novo | aplica sem erro |
| Passo 3 | o mesmo conflito, agora com `V4` e `V5`. Renumere para `V6` e `V7` | aplica `Alfa1` e `Alfa2` sem erro, mesmo sendo mais antigas que `Beta1` e `Gama1` |

**Variação Flyway (versão por data e hora):** refaça a prática nomeando as migrations com data e hora, por exemplo `V20260930_1401__alfa1_cor.sql`. Os números deixam de colidir. Agora, no passo 3, as versões de `featAlfa` são **menores** que as já aplicadas:

- com `outOfOrder = false` (padrão do `flyway.toml`), elas ficam como `Ignored` e o `migrate` falha na validação;
- troque para `outOfOrder = true`, rode de novo e compare, no `flyway_schema_history`, a ordem de `version` com a de `installed_rank`.

### Responda

1. Todas as alterações foram aplicadas no fim? Confira as colunas da `TABELATESTE`.
2. Em que ordem as migrations foram de fato aplicadas? A tabela de histórico da sua ferramenta permite saber isso? (Compare `installed_rank`/`installed_on` do Flyway com as colunas da `__EFMigrationsHistory`.)
3. Qual comportamento é mais seguro: o Flyway parar com erro ou o EF Core aplicar em silêncio? Em que situação aplicar fora de ordem quebraria o banco? (Pense em `Alfa2` dependendo de uma coluna criada em `Beta1`.)
4. Na trilha Flyway, qual convenção de versão a equipe deveria adotar (número sequencial ou data e hora) e por quê?

---

## Prática 3: trocar uma coluna sem quebrar a versão anterior (≈ 20 min)

**Objetivo:** substituir a coluna `DESCRICAO` por `NOME` com a técnica **expand/contract**, em dois Pull Requests, sem que nenhuma versão em uso fique incompatível com o banco.

### PR 1: expand (a coluna nova convive com a antiga)

```bash
git switch develop && git switch -c featExpandNome
```

| | Flyway | EF Core |
|---|---|---|
| Migration | `V<próxima>__expand_coluna_nome.sql`:<br>`ALTER TABLE TABELATESTE ADD COLUMN NOME VARCHAR(100);`<br>`UPDATE TABELATESTE SET NOME = DESCRICAO WHERE NOME IS NULL;` | `dotnet ef migrations add EXPAND_COLUNA_NOME` com os mesmos dois comandos em `migrationBuilder.Sql(...)` (no SQL Server, `ADD` sem `COLUMN`) |
| Consumidores | altere `sql/R__vw_tabelateste.sql` para ler `NOME` em vez de `DESCRICAO` | — |

Faça merge em `develop` e aplique. As duas colunas existem; quem ainda usa `DESCRICAO` continua funcionando.

### PR 2: contract (remove a coluna antiga)

```bash
git switch develop && git switch -c featContractNome
```

| | Flyway | EF Core |
|---|---|---|
| Migration | `V<próxima>__contract_remove_descricao.sql`:<br>`ALTER TABLE TABELATESTE DROP COLUMN DESCRICAO;` | `dotnet ef migrations add CONTRACT_REMOVE_DESCRICAO` com<br>`migrationBuilder.Sql("ALTER TABLE TABELATESTE DROP COLUMN DESCRICAO");` |
| Script para revisão | `flyway -configFiles=flyway.toml info` antes do merge | `dotnet ef migrations script --idempotent -o migrate.sql` e anexe ao PR |

**Experimento (Flyway):** antes de fazer o PR 1, tente aplicar só o `DROP COLUMN` com a view ainda lendo `DESCRICAO`. O SQLite recusa (`error in view VW_TABELATESTE after drop column: no such column: DESCRICAO`) e o Flyway desfaz a migration. É o motivo de os consumidores migrarem para a coluna nova **antes** do contract. Apague o arquivo do experimento antes de seguir.

### Responda (na descrição do PR 2)

1. Entre o PR 1 e o PR 2, quais versões do código e da view funcionam com o banco? E depois do PR 2?
2. As migrations repetíveis (`R__`) rodam **depois** das versionadas. Por que isso obriga a atualizar a view no PR 1 e não no PR 2?
3. Se for preciso desfazer o PR 2, os valores de `DESCRICAO` voltam? O que deveria existir antes de rodar um `DROP COLUMN` em produção?

---

## Referências

- Flyway: migrations, `outOfOrder`, `validate`. https://documentation.red-gate.com/fd
- Microsoft. *Migrations in Team Environments* (EF Core). https://learn.microsoft.com/ef/core/managing-schemas/migrations/teams
- SATO, D. *ParallelChange*. https://martinfowler.com/bliki/ParallelChange.html
- AMBLER, S. W.; SADALAGE, P. J. *Refactoring Databases*. Addison-Wesley, 2006.
