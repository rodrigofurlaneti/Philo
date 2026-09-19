-- Exemplos para adaptar a statements parametrizados da API.
-- @organization_id, @conversation_id e @user_id devem vir do contexto AUTORIZADO.
-- Nao executar o arquivo inteiro como uma migracao.
USE customer_chat;

-- 1. Autorizacao basica de participante ativo (tambem antes de anexos/recibos).
SELECT p.user_id, ou.role
FROM conversation_participants p
JOIN organization_users ou
  ON ou.organization_id = p.organization_id AND ou.user_id = p.user_id
JOIN users u ON u.id = p.user_id
JOIN organizations o ON o.id = p.organization_id
WHERE p.organization_id = @organization_id
  AND p.conversation_id = @conversation_id
  AND p.user_id = @user_id
  AND p.left_at IS NULL
  AND ou.status = 'active' AND u.status = 'active' AND o.status = 'active';

-- 2. Historico. Primeira pagina: @cursor_sent_at e @cursor_id = NULL.
-- Manter as duas partes do cursor para desempatar timestamps iguais.
SELECT m.id, m.sender_id, m.message_type,
       CASE WHEN m.deleted_at IS NULL THEN m.body ELSE NULL END AS body,
       m.reply_to_id, m.sent_at, m.edited_at, m.deleted_at
FROM messages m
JOIN conversation_participants p
  ON p.organization_id = m.organization_id
 AND p.conversation_id = m.conversation_id
 AND p.user_id = @user_id AND p.left_at IS NULL
LEFT JOIN message_user_preferences pref
  ON pref.organization_id = m.organization_id
 AND pref.conversation_id = m.conversation_id
 AND pref.message_id = m.id AND pref.user_id = @user_id
WHERE m.organization_id = @organization_id
  AND m.conversation_id = @conversation_id
  AND (m.expires_at IS NULL OR m.expires_at > UTC_TIMESTAMP(6))
  AND pref.hidden_at IS NULL
  AND (@cursor_sent_at IS NULL
       OR m.sent_at < @cursor_sent_at
       OR (m.sent_at = @cursor_sent_at AND m.id < @cursor_id))
ORDER BY m.sent_at DESC, m.id DESC
LIMIT 50;

-- 3. Nao lidas: apenas mensagens destinadas a este usuario, ainda visiveis.
SELECT COUNT(*) AS unread_count
FROM message_receipts r
JOIN messages m
  ON m.organization_id = r.organization_id
 AND m.conversation_id = r.conversation_id AND m.id = r.message_id
LEFT JOIN message_user_preferences pref
  ON pref.organization_id = r.organization_id
 AND pref.conversation_id = r.conversation_id
 AND pref.message_id = r.message_id AND pref.user_id = r.user_id
WHERE r.organization_id = @organization_id
  AND r.conversation_id = @conversation_id AND r.user_id = @user_id
  AND r.read_at IS NULL AND m.deleted_at IS NULL
  AND (m.expires_at IS NULL OR m.expires_at > UTC_TIMESTAMP(6))
  AND pref.hidden_at IS NULL;

-- 4. Previa individual: reutilize a consulta de historico com LIMIT 1.
-- Mensagem apagada deve aparecer como tombstone, sem corpo, anexos ou texto citado.
-- Nao usar um last_message_id global: ocultar uma mensagem e uma preferencia individual.

-- 5. Registrar leitura de um recibo existente (apos autorizar acesso).
-- API valida que a mensagem e visivel e que o recibo pertence ao usuario autenticado.
-- O banco gera os horarios; o navegador nao escolhe estas datas.
UPDATE message_receipts
SET delivered_at = COALESCE(delivered_at, UTC_TIMESTAMP(6)),
    read_at = COALESCE(read_at, UTC_TIMESTAMP(6))
WHERE organization_id = @organization_id
  AND conversation_id = @conversation_id
  AND message_id = @message_id AND user_id = @user_id;

-- 6. Primeira atribuicao de atendimento que estava sem equipe.
-- Executar DENTRO da transacao de atribuicao, apos bloquear conversations FOR UPDATE,
-- validar papel agent/admin e inserir/reativar o atendente em conversation_participants.
-- As mensagens sem nenhum destinatario passam a ter o primeiro atendente como destino.
-- Nunca substituir recibos antigos nem adicionar a si proprio como destinatario.
INSERT INTO message_receipts (organization_id, conversation_id, message_id, user_id)
SELECT m.organization_id, m.conversation_id, m.id, @agent_id
FROM messages m
LEFT JOIN message_receipts r
  ON r.organization_id = m.organization_id
 AND r.conversation_id = m.conversation_id AND r.message_id = m.id
WHERE m.organization_id = @organization_id
  AND m.conversation_id = @conversation_id
  AND m.sender_id <> @agent_id
  AND m.deleted_at IS NULL
  AND (m.expires_at IS NULL OR m.expires_at > UTC_TIMESTAMP(6))
  AND r.message_id IS NULL;

-- 7. Fila da equipe; endpoint restrito a agent/admin da empresa.
SELECT id, customer_id, assigned_to, purpose, subject, status, priority, last_activity_at
FROM conversations
WHERE organization_id = @organization_id AND status = 'open'
ORDER BY last_activity_at DESC, id DESC
LIMIT 50;
