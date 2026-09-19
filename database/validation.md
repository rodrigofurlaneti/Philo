# Cenários de aceitação

Status: especificados para execução em banco de teste; não executados neste ambiente.

Criar duas organizações, usuários distintos em cada uma, um cliente e dois atendentes na primeira, um produto por organização e duas conversas na primeira. Executar com MySQL 8.4, modo estrito e UTC.

| Cenário | Resultado esperado | Camada responsável |
|---|---|---|
| Instalar schema.sql em banco novo | 13 tabelas, 1 trigger e 1 view; nenhuma falha | MySQL |
| Vincular produto da empresa B à conversa da A | FK rejeita | MySQL |
| Adicionar participante sem vínculo com a empresa | FK rejeita | MySQL |
| Enviar como usuário que não participa | FK rejeita | MySQL |
| Enviar como participante com left_at preenchido | Acesso negado | API |
| Responder à mensagem de outra conversa | FK rejeita | MySQL |
| Repetir mesmo client_message_id, conversa e autor | UNIQUE rejeita segundo INSERT; API retorna a primeira mensagem | MySQL/API |
| Enviar por cliente com dois outros participantes ativos | Exatamente dois recibos, um evento de outbox e atividade atualizada | Trigger |
| Falhar após inserir mensagem, antes do COMMIT | Mensagem, recibos, atividade e outbox revertidos | Transação |
| Enviar sem outros participantes | Zero recibos; view retorna sent, nunca read | Trigger/view |
| Atribuir primeiro atendente a conversa sem destinatários | Recibos das mensagens pendentes criados para esse atendente | API/queries.sql |
| Registrar entrega de um entre dois destinatários | Status global continua sent | View |
| Registrar entrega de ambos e leitura de apenas um | Status global delivered | View |
| Registrar leitura de ambos | Status global read | View |
| Registrar read_at sem delivered_at ou antes dele | CHECK rejeita | MySQL |
| Consultar histórico de outro usuário/empresa sem acesso | Acesso negado, inclusive por URL de anexo | API |
| Favoritar/ocultar mensagem como usuário A | Preferências de B permanecem intactas | API/modelo |
| Apagar para todos sem limpar body | CHECK rejeita | MySQL |
| Apagar corretamente uma mensagem | Tombstone sem corpo/anexos, inclusive nas citações e prévias | API/MySQL |
| Expirar mensagem ainda não lida | Some do histórico, prévia e contagem; anexo deixa de ser autorizado | API/consultas |
| Fechar sem closed_at ou manter closed_at ao reabrir | CHECK rejeita | MySQL |
| Enviar anexo com zero bytes ou mais de 100 MiB | CHECK rejeita | MySQL |
| Enviar tipo attachment sem arquivo | API reverte transação | API |
| Consultar anexo pending/rejected ou chave de outro usuário | Acesso negado | API/storage |
| Publicador cair após publicar e antes de marcar evento | Evento pode repetir; consumidor deduplica pelo id | Worker/cliente |
| Duas conexões enviarem mesma chave simultaneamente | Uma mensagem, um evento, um conjunto de recibos | Transação/UNIQUE |
| Uma conexão remover participante enquanto outra envia | Bloqueio da conversa serializa ambas; destinatários coerentes com ordem efetiva | API/transação |
| Transferir conversa para outro atendente | Novo participante, responsável atualizado, evento de auditoria; histórico preservado | API/transação |
| Paginar mensagens com timestamps iguais | Sem pular/repetir linhas entre páginas usando o par sent_at/id | Consulta |

Não tratar apenas o sucesso de CREATE TABLE como prova de autorização, isolamento de leitura, entrega em tempo real ou limpeza de arquivos. Esses cenários exigem a API e seus workers.
