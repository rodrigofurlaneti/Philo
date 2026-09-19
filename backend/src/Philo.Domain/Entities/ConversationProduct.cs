using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Vínculo (organization_id, conversation_id, product_id) com o nome preservado no momento do vínculo.</summary>
    public sealed class ConversationProduct
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long ProductId { get; private set; }
        public string ProductNameSnapshot { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }

        public Conversation? Conversation { get; private set; }
        public Product? Product { get; private set; }

        private ConversationProduct() { }

        private ConversationProduct(long organizationId, long conversationId, long productId, string productNameSnapshot)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            ProductId = productId;
            ProductNameSnapshot = productNameSnapshot;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<ConversationProduct> Create(long organizationId, long conversationId, long productId, string productNameSnapshot)
        {
            if (string.IsNullOrWhiteSpace(productNameSnapshot))
                return Result.Failure<ConversationProduct>(Error.Validation("ConversationProduct.NameRequired", "O nome do produto deve ser preservado no vínculo."));

            return Result.Success(new ConversationProduct(organizationId, conversationId, productId, productNameSnapshot.Trim()));
        }
    }
}
