-- Equivalente à migration EF Core 20250923195704_TABELA_TESTE.
-- Uma migration versionada roda uma única vez por banco: não use IF NOT EXISTS.
CREATE TABLE TABELATESTE (
    ID        INT PRIMARY KEY,
    DESCRICAO VARCHAR(100),
    DATAHORA  TIMESTAMP
);
