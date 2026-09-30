-- Migration REPETÍVEL (prefixo R__): roda de novo sempre que este arquivo mudar.
-- Por isso, e só aqui, o script precisa aguentar reexecução (DROP ... IF EXISTS).
-- Repetíveis rodam depois de todas as versionadas pendentes.
DROP VIEW IF EXISTS VW_TABELATESTE;

CREATE VIEW VW_TABELATESTE AS
SELECT ID, DESCRICAO, DATAHORA
FROM TABELATESTE;
