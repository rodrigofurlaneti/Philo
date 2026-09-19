-- Chat de vendas e suporte | modelo inicial para MySQL 8.4 / InnoDB.
-- Execute uma vez em um banco novo. Nao e uma migracao do modelo anterior.
-- Todas as conexoes da API devem usar UTC e modo SQL estrito.
-- Leia README.md: permissoes e fluxos transacionais sao parte do contrato.
SET NAMES utf8mb4;
SET time_zone = '+00:00';

CREATE DATABASE IF NOT EXISTS philodb
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE philodb;

CREATE TABLE organizations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    status ENUM('active','suspended') NOT NULL DEFAULT 'active',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
) ENGINE=InnoDB;

-- Identidade global. Telefone e email opcionais permitem visitantes.
-- Autenticacao pertence ao site/provedor; estes contatos nao sao credenciais.
CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    display_name VARCHAR(100) NOT NULL,
    email VARCHAR(254) NULL,
    phone_e164 VARCHAR(16) CHARACTER SET ascii COLLATE ascii_bin NULL,
    avatar_object_key VARCHAR(512) COLLATE utf8mb4_bin NULL,
    status ENUM('active','suspended','anonymized') NOT NULL DEFAULT 'active',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6)
) ENGINE=InnoDB;

-- Papel por empresa; o mesmo usuario pode pertencer a varias empresas.
CREATE TABLE organization_users (
    organization_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    role ENUM('customer','agent','admin') NOT NULL DEFAULT 'customer',
    status ENUM('active','suspended') NOT NULL DEFAULT 'active',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (organization_id, user_id),
    CONSTRAINT fk_ou_org FOREIGN KEY (organization_id) REFERENCES organizations(id),
    CONSTRAINT fk_ou_user FOREIGN KEY (user_id) REFERENCES users(id),
    INDEX idx_ou_user (user_id, organization_id),
    INDEX idx_ou_team (organization_id, role, status)
) ENGINE=InnoDB;

-- Catalogo minimo. Se o site ja tem produtos, adaptar para o catalogo existente.
CREATE TABLE products (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    sku VARCHAR(64) COLLATE utf8mb4_bin NULL,
    name VARCHAR(200) NOT NULL,
    page_url VARCHAR(2048) NULL,
    status ENUM('active','inactive') NOT NULL DEFAULT 'active',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT uq_product_org UNIQUE (organization_id, id),
    CONSTRAINT uq_product_sku UNIQUE (organization_id, sku),
    CONSTRAINT fk_product_org FOREIGN KEY (organization_id) REFERENCES organizations(id)
) ENGINE=InnoDB;

-- Um atendimento por assunto; varios atendimentos para o mesmo cliente sao validos.
CREATE TABLE conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    assigned_to BIGINT UNSIGNED NULL,
    created_by BIGINT UNSIGNED NOT NULL,
    purpose ENUM('sales','support') NOT NULL,
    subject VARCHAR(200) NULL,
    status ENUM('open','waiting_customer','waiting_team','closed') NOT NULL DEFAULT 'open',
    priority ENUM('low','normal','high','urgent') NOT NULL DEFAULT 'normal',
    source_page_url VARCHAR(2048) NULL,
    last_activity_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),
    closed_at DATETIME(6) NULL,
    CONSTRAINT uq_conversation_org UNIQUE (organization_id, id),
    CONSTRAINT fk_conversation_customer FOREIGN KEY (organization_id, customer_id)
        REFERENCES organization_users(organization_id, user_id),
    CONSTRAINT fk_conversation_agent FOREIGN KEY (organization_id, assigned_to)
        REFERENCES organization_users(organization_id, user_id),
    CONSTRAINT fk_conversation_creator FOREIGN KEY (organization_id, created_by)
        REFERENCES organization_users(organization_id, user_id),
    CONSTRAINT ck_conversation_closed CHECK (
        (status = 'closed' AND closed_at IS NOT NULL AND closed_at >= created_at)
        OR (status <> 'closed' AND closed_at IS NULL)
    ),
    INDEX idx_conversation_queue (organization_id, status, last_activity_at DESC, id DESC),
    INDEX idx_conversation_assigned (organization_id, assigned_to, status, last_activity_at DESC),
    INDEX idx_conversation_customer (organization_id, customer_id, created_at DESC)
) ENGINE=InnoDB;

CREATE TABLE conversation_products (
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    product_name_snapshot VARCHAR(200) NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (organization_id, conversation_id, product_id),
    CONSTRAINT fk_cp_conversation FOREIGN KEY (organization_id, conversation_id)
        REFERENCES conversations(organization_id, id),
    CONSTRAINT fk_cp_product FOREIGN KEY (organization_id, product_id)
        REFERENCES products(organization_id, id)
) ENGINE=InnoDB;

-- joined_at = primeira entrada. A API administra saida e reentrada sob bloqueio.
-- Todos os participantes ativos podem consultar o historico desta conversa.
CREATE TABLE conversation_participants (
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    joined_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    left_at DATETIME(6) NULL,
    archived_at DATETIME(6) NULL,
    pinned_at DATETIME(6) NULL,
    muted_until DATETIME(6) NULL,
    PRIMARY KEY (organization_id, conversation_id, user_id),
    CONSTRAINT fk_participant_conversation FOREIGN KEY (organization_id, conversation_id)
        REFERENCES conversations(organization_id, id),
    CONSTRAINT fk_participant_user FOREIGN KEY (organization_id, user_id)
        REFERENCES organization_users(organization_id, user_id),
    CONSTRAINT ck_participant_dates CHECK (left_at IS NULL OR left_at >= joined_at),
    INDEX idx_participant_inbox (organization_id, user_id, left_at, archived_at, conversation_id)
) ENGINE=InnoDB;

CREATE TABLE messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    sender_id BIGINT UNSIGNED NOT NULL,
    -- UUID enviado pelo cliente; a mesma tentativa deve reutilizar este valor.
    client_message_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    message_type ENUM('text','attachment') NOT NULL DEFAULT 'text',
    body TEXT NULL COMMENT 'Texto ou legenda; nao e um campo de JSON arbitrario',
    reply_to_id BIGINT UNSIGNED NULL,
    sent_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    edited_at DATETIME(6) NULL,
    deleted_at DATETIME(6) NULL COMMENT 'Apagada para todos; body deve ficar NULL',
    expires_at DATETIME(6) NULL,
    CONSTRAINT uq_message_scope UNIQUE (organization_id, conversation_id, id),
    CONSTRAINT uq_message_retry UNIQUE (
        organization_id, conversation_id, sender_id, client_message_id
    ),
    CONSTRAINT fk_message_sender FOREIGN KEY (organization_id, conversation_id, sender_id)
        REFERENCES conversation_participants(organization_id, conversation_id, user_id),
    CONSTRAINT fk_message_reply FOREIGN KEY (organization_id, conversation_id, reply_to_id)
        REFERENCES messages(organization_id, conversation_id, id),
    CONSTRAINT ck_message_body CHECK (
        deleted_at IS NOT NULL OR message_type = 'attachment'
        OR (body IS NOT NULL AND CHAR_LENGTH(TRIM(body)) > 0)
    ),
    CONSTRAINT ck_message_deleted CHECK (
        deleted_at IS NULL OR (body IS NULL AND deleted_at >= sent_at)
    ),
    CONSTRAINT ck_message_edited CHECK (edited_at IS NULL OR edited_at >= sent_at),
    CONSTRAINT ck_message_expiry CHECK (expires_at IS NULL OR expires_at > sent_at),
    INDEX idx_message_history (organization_id, conversation_id, sent_at DESC, id DESC),
    INDEX idx_message_expiry (expires_at, id)
) ENGINE=InnoDB;

-- Binarios em armazenamento privado; gerar URL temporaria apos autorizar acesso.
CREATE TABLE message_attachments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_id BIGINT UNSIGNED NOT NULL,
    original_name VARCHAR(255) NOT NULL,
    mime_type VARCHAR(127) CHARACTER SET ascii NOT NULL,
    file_size_bytes BIGINT UNSIGNED NOT NULL,
    storage_bucket VARCHAR(63) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    storage_key VARCHAR(512) COLLATE utf8mb4_bin NOT NULL,
    sha256 BINARY(32) NOT NULL,
    scan_status ENUM('pending','clean','rejected') NOT NULL DEFAULT 'pending',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT uq_attachment_object UNIQUE (storage_bucket, storage_key),
    CONSTRAINT fk_attachment_message FOREIGN KEY (organization_id, conversation_id, message_id)
        REFERENCES messages(organization_id, conversation_id, id),
    CONSTRAINT ck_attachment_size CHECK (file_size_bytes > 0 AND file_size_bytes <= 104857600),
    INDEX idx_attachment_message (organization_id, conversation_id, message_id)
) ENGINE=InnoDB;

-- Destinatarios esperados sao gravados no envio, mesmo sem entrega/leitura.
CREATE TABLE message_receipts (
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    delivered_at DATETIME(6) NULL,
    read_at DATETIME(6) NULL,
    PRIMARY KEY (organization_id, conversation_id, message_id, user_id),
    CONSTRAINT fk_receipt_message FOREIGN KEY (organization_id, conversation_id, message_id)
        REFERENCES messages(organization_id, conversation_id, id),
    CONSTRAINT fk_receipt_participant FOREIGN KEY (organization_id, conversation_id, user_id)
        REFERENCES conversation_participants(organization_id, conversation_id, user_id),
    CONSTRAINT ck_receipt_read CHECK (
        read_at IS NULL OR (delivered_at IS NOT NULL AND read_at >= delivered_at)
    ),
    INDEX idx_receipt_unread (organization_id, user_id, read_at, conversation_id, message_id)
) ENGINE=InnoDB;

-- Favoritar e apagar apenas para mim sao preferencias individuais.
CREATE TABLE message_user_preferences (
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    starred_at DATETIME(6) NULL,
    hidden_at DATETIME(6) NULL,
    PRIMARY KEY (organization_id, conversation_id, message_id, user_id),
    CONSTRAINT fk_preference_message FOREIGN KEY (organization_id, conversation_id, message_id)
        REFERENCES messages(organization_id, conversation_id, id),
    CONSTRAINT fk_preference_participant FOREIGN KEY (organization_id, conversation_id, user_id)
        REFERENCES conversation_participants(organization_id, conversation_id, user_id),
    INDEX idx_preference_user (organization_id, user_id, starred_at)
) ENGINE=InnoDB;

-- Auditoria interna: nao expor automaticamente ao cliente nem tratar como mensagem.
CREATE TABLE conversation_events (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    actor_id BIGINT UNSIGNED NULL COMMENT 'NULL = processo do sistema',
    event_type ENUM('created','assigned','status_changed','participant_joined','participant_left') NOT NULL,
    details JSON NOT NULL COMMENT 'Dados estruturados validados pela API; sem conteudo de mensagens',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT fk_event_conversation FOREIGN KEY (organization_id, conversation_id)
        REFERENCES conversations(organization_id, id),
    CONSTRAINT fk_event_actor FOREIGN KEY (organization_id, actor_id)
        REFERENCES organization_users(organization_id, user_id),
    INDEX idx_event_history (organization_id, conversation_id, created_at, id)
) ENGINE=InnoDB;

-- Fila transacional para WebSocket/notificacoes; nao significa entrega ao usuario.
CREATE TABLE outbox_events (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    organization_id BIGINT UNSIGNED NOT NULL,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_id BIGINT UNSIGNED NULL,
    event_type VARCHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    available_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    published_at DATETIME(6) NULL,
    attempts INT UNSIGNED NOT NULL DEFAULT 0,
    CONSTRAINT fk_outbox_conversation FOREIGN KEY (organization_id, conversation_id)
        REFERENCES conversations(organization_id, id),
    CONSTRAINT fk_outbox_message FOREIGN KEY (organization_id, conversation_id, message_id)
        REFERENCES messages(organization_id, conversation_id, id),
    INDEX idx_outbox_pending (published_at, available_at, id)
) ENGINE=InnoDB;

-- API deve bloquear a conversa ANTES de enviar ou alterar participantes.
-- Esta trigger materializa destinatarios e evento na mesma transacao da mensagem.
DELIMITER $$
CREATE TRIGGER trg_message_created AFTER INSERT ON messages
FOR EACH ROW
BEGIN
    INSERT INTO message_receipts (organization_id, conversation_id, message_id, user_id)
    SELECT NEW.organization_id, NEW.conversation_id, NEW.id, p.user_id
    FROM conversation_participants p
    WHERE p.organization_id = NEW.organization_id
      AND p.conversation_id = NEW.conversation_id
      AND p.left_at IS NULL
      AND p.user_id <> NEW.sender_id;

    UPDATE conversations
    SET last_activity_at = GREATEST(last_activity_at, NEW.sent_at)
    WHERE organization_id = NEW.organization_id AND id = NEW.conversation_id;

    INSERT INTO outbox_events (organization_id, conversation_id, message_id, event_type)
    VALUES (NEW.organization_id, NEW.conversation_id, NEW.id, 'message.created');
END$$
DELIMITER ;

-- View administrativa. A API precisa autorizar e filtrar empresa/conversa.
-- Sem destinatarios => sent. Nunca inferir read pela igualdade 0 = 0.
CREATE VIEW vw_message_delivery AS
SELECT
    m.organization_id, m.conversation_id, m.id AS message_id,
    COUNT(r.user_id) AS expected_recipients,
    COALESCE(SUM(r.delivered_at IS NOT NULL), 0) AS delivered_count,
    COALESCE(SUM(r.read_at IS NOT NULL), 0) AS read_count,
    CASE
        WHEN m.deleted_at IS NOT NULL THEN 'deleted'
        WHEN m.expires_at IS NOT NULL AND m.expires_at <= UTC_TIMESTAMP(6) THEN 'expired'
        WHEN COUNT(r.user_id) = 0 THEN 'sent'
        WHEN SUM(r.read_at IS NOT NULL) = COUNT(r.user_id) THEN 'read'
        WHEN SUM(r.delivered_at IS NOT NULL) = COUNT(r.user_id) THEN 'delivered'
        ELSE 'sent'
    END AS delivery_status
FROM messages m
LEFT JOIN message_receipts r
    ON r.organization_id = m.organization_id
   AND r.conversation_id = m.conversation_id
   AND r.message_id = m.id
GROUP BY m.organization_id, m.conversation_id, m.id, m.deleted_at, m.expires_at;
