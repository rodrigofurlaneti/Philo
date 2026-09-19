using Philo.Domain.Primitives;

namespace Philo.Domain.Errors
{
    /// <summary>
    /// Catálogo central de falhas esperadas do domínio. Mantém os códigos estáveis
    /// para que a WebApi possa traduzi-los para status HTTP de forma consistente.
    /// </summary>
    public static class DomainErrors
    {
        public static class Organization
        {
            public static Error NotFound(long id) =>
                Primitives.Error.NotFound("Organization.NotFound", $"Organização {id} não encontrada.");
            public static readonly Error Suspended =
                Primitives.Error.Forbidden("Organization.Suspended", "Organização suspensa.");
        }

        public static class User
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("User.NotFound", "Usuário não encontrado.");
            public static readonly Error Suspended =
                Primitives.Error.Forbidden("User.Suspended", "Usuário suspenso.");
            public static readonly Error Anonymized =
                Primitives.Error.Forbidden("User.Anonymized", "Usuário anonimizado.");
        }

        public static class Membership
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Membership.NotFound", "Usuário não vinculado a esta organização.");
            public static readonly Error Suspended =
                Primitives.Error.Forbidden("Membership.Suspended", "Vínculo do usuário com a organização está suspenso.");
            public static readonly Error WrongRole =
                Primitives.Error.Forbidden("Membership.WrongRole", "Papel do usuário não permite esta operação.");
        }

        public static class Product
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Product.NotFound", "Produto não encontrado.");
            public static readonly Error DuplicateSku =
                Primitives.Error.Conflict("Product.DuplicateSku", "Já existe um produto com este SKU nesta organização.");
        }

        public static class Conversation
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Conversation.NotFound", "Conversa não encontrada.");
            public static readonly Error AlreadyClosed =
                Primitives.Error.Conflict("Conversation.AlreadyClosed", "Conversa já está encerrada.");
            public static readonly Error NotClosed =
                Primitives.Error.Conflict("Conversation.NotClosed", "Conversa não está encerrada.");
            public static readonly Error CustomerMustBeCustomerRole =
                Primitives.Error.Validation("Conversation.CustomerMustBeCustomerRole", "O cliente informado não possui papel 'customer' na organização.");
            public static readonly Error AssigneeMustBeStaff =
                Primitives.Error.Validation("Conversation.AssigneeMustBeStaff", "O responsável informado precisa ter papel 'agent' ou 'admin'.");
        }

        public static class Participant
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Participant.NotFound", "Participante não encontrado na conversa.");
            public static readonly Error AlreadyActive =
                Primitives.Error.Conflict("Participant.AlreadyActive", "Usuário já participa ativamente desta conversa.");
            public static readonly Error NotActive =
                Primitives.Error.Forbidden("Participant.NotActive", "Usuário não é participante ativo desta conversa.");
            public static readonly Error CannotRemoveCustomer =
                Primitives.Error.Forbidden("Participant.CannotRemoveCustomer", "O cliente titular da conversa não pode ser removido.");
            public static readonly Error InsufficientRole =
                Primitives.Error.Forbidden("Participant.InsufficientRole", "Apenas funcionários autorizados podem gerenciar participantes.");
        }

        public static class Message
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Message.NotFound", "Mensagem não encontrada.");
            public static readonly Error ConversationClosed =
                Primitives.Error.Forbidden("Message.ConversationClosed", "Não é possível enviar mensagens em uma conversa encerrada.");
            public static readonly Error SenderNotActiveParticipant =
                Primitives.Error.Forbidden("Message.SenderNotActiveParticipant", "Remetente não é participante ativo desta conversa.");
            public static readonly Error EmptyBody =
                Primitives.Error.Validation("Message.EmptyBody", "O texto da mensagem não pode ser vazio.");
            public static readonly Error AttachmentRequired =
                Primitives.Error.Validation("Message.AttachmentRequired", "Mensagens do tipo attachment exigem ao menos um arquivo.");
            public static readonly Error ReplyMustBeSameConversation =
                Primitives.Error.Validation("Message.ReplyMustBeSameConversation", "A mensagem respondida deve pertencer à mesma conversa.");
            public static readonly Error ReplyToSelf =
                Primitives.Error.Validation("Message.ReplyToSelf", "Uma mensagem não pode responder a si mesma.");
            public static readonly Error ReplyNotVisible =
                Primitives.Error.NotFound("Message.ReplyNotVisible", "Mensagem respondida não está acessível.");
            public static readonly Error AlreadyDeleted =
                Primitives.Error.Conflict("Message.AlreadyDeleted", "Mensagem já foi apagada.");
            public static readonly Error NotAuthor =
                Primitives.Error.Forbidden("Message.NotAuthor", "Apenas o autor pode editar ou apagar a mensagem.");
            public static readonly Error DuplicateClientMessageId =
                Primitives.Error.Conflict("Message.DuplicateClientMessageId", "Esta mensagem já foi registrada (idempotência).");
        }

        public static class Attachment
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Attachment.NotFound", "Anexo não encontrado.");
            public static readonly Error InvalidSize =
                Primitives.Error.Validation("Attachment.InvalidSize", "Tamanho do arquivo deve ser maior que zero e no máximo 100 MiB.");
            public static readonly Error NotReady =
                Primitives.Error.Forbidden("Attachment.NotReady", "Arquivo ainda não passou pela verificação de segurança.");
            public static readonly Error Rejected =
                Primitives.Error.Forbidden("Attachment.Rejected", "Arquivo rejeitado pela verificação de segurança.");
        }

        public static class Receipt
        {
            public static readonly Error NotFound =
                Primitives.Error.NotFound("Receipt.NotFound", "Recibo não encontrado para este usuário.");
            public static readonly Error ReadRequiresDelivery =
                Primitives.Error.Validation("Receipt.ReadRequiresDelivery", "Leitura exige confirmação de entrega prévia ou simultânea.");
        }

        public static class Auth
        {
            public static readonly Error InvalidCredentials =
                Primitives.Error.Unauthorized("Auth.InvalidCredentials", "Credenciais inválidas.");
            public static readonly Error Unauthenticated =
                Primitives.Error.Unauthorized("Auth.Unauthenticated", "Requisição não autenticada.");
        }
    }
}
