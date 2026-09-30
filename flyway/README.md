# Trilha Flyway

Migrations escritas em **SQL puro**. Não é preciso saber C#, Java nem outra linguagem: só `CREATE TABLE`, `ALTER TABLE`, `INSERT`.

O banco padrão é **SQLite**: um único arquivo (`gcodb.db`) criado nesta pasta no primeiro `migrate`. Não há servidor de banco para instalar.

## 1. Instalar o Flyway (uma vez)

Escolha **uma** das opções.

**Opção A: CLI (recomendada no laboratório)**

1. Baixe o *Flyway Community* (Command-line) para o seu sistema: https://www.red-gate.com/products/flyway/community/ (documentação: https://documentation.red-gate.com/fd)
2. Descompacte e coloque a pasta no `PATH`. O pacote para Windows, macOS e Linux já inclui o Java; não é preciso instalar mais nada.
3. Teste com `flyway --version`.

**Opção B: Docker**

Troque `flyway` por:

```bash
docker run --rm -v "$PWD:/flyway/project" -w /flyway/project redgate/flyway
```

No PowerShell, use `${PWD}` no lugar de `$PWD`.

## 2. Comandos

Rode **sempre de dentro da pasta `flyway/`**:

```bash
cd flyway
flyway -configFiles=flyway.toml info       # o que já foi aplicado e o que está pendente
flyway -configFiles=flyway.toml migrate    # aplica as pendentes
flyway -configFiles=flyway.toml validate   # confere se nenhum arquivo aplicado foi alterado
```

| Comando | Para que serve |
|---|---|
| `info` | lista cada migration com o estado: `Success`, `Pending`, `Failed`, `Ignored`, `Missing` |
| `migrate` | aplica, em ordem, as migrations que ainda não estão no histórico |
| `validate` | recalcula o *checksum* dos arquivos e compara com o histórico |
| `repair` | corrige o histórico depois de uma falha (use só quando o professor indicar) |
| `clean` | **apaga tudo do banco**. Está desativado por padrão; não use |

## 3. Como nomear uma migration

```text
V3__adiciona_coluna_ativo.sql
│ │ └─ descrição (use _ no lugar de espaço)
│ └─── dois sublinhados
└───── V + número da versão
```

| Prefixo | Tipo | Quando roda |
|---|---|---|
| `V` | versionada | uma única vez, em ordem de versão |
| `R` | repetível | toda vez que o conteúdo do arquivo muda, depois das versionadas |

- A versão precisa ser **única**. Duas branches criando `V3` em paralelo geram conflito; isso é assunto da Prática 2.
- Migrations `V` **não levam `IF NOT EXISTS`**, porque o histórico garante que rodam uma vez.
- Migrations `R` (views, functions) **precisam** aguentar reexecução: veja `sql/R__vw_tabelateste.sql`.

## 4. Onde o Flyway registra o que foi executado

Na tabela `flyway_schema_history`, dentro do próprio banco:

```sql
SELECT installed_rank, version, description, script, checksum, installed_on, success
FROM flyway_schema_history
ORDER BY installed_rank;
```

| Coluna | Significado |
|---|---|
| `installed_rank` | ordem real de aplicação |
| `version` / `description` | tirados do nome do arquivo |
| `script` | nome do arquivo executado |
| `checksum` | "impressão digital" do conteúdo no momento da execução |
| `installed_on` / `installed_by` | quando e por qual usuário |
| `success` | se terminou sem erro |

Para abrir o `gcodb.db`, use o **DB Browser for SQLite** (https://sqlitebrowser.org) ou a extensão *SQLite Viewer* do VS Code.

## 5. Opcional: PostgreSQL

Com um PostgreSQL rodando (por exemplo `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=gcodb postgres:17`):

```bash
flyway -configFiles=flyway.toml -url=jdbc:postgresql://localhost:5432/gcodb -user=postgres -password=postgres migrate
```

Os três scripts deste repositório funcionam em SQLite e PostgreSQL.

## 6. Erros comuns

| Mensagem (resumo) | Causa | O que fazer |
|---|---|---|
| `Validate failed: Migration checksum mismatch` | um arquivo já aplicado foi editado | desfaça a edição e crie uma migration nova |
| `Found more than one migration with version X` | duas migrations com o mesmo número (em geral, após um merge) | renumere a sua para a próxima versão livre |
| `Detected resolved migration not applied to database` | chegou uma versão menor que a última aplicada | renumere para uma versão maior (ou veja `outOfOrder` na Prática 2) |
| `Unable to obtain connection` | caminho ou URL do banco errado | confira se está dentro da pasta `flyway/` |
