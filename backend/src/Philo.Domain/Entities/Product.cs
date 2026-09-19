using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class Product : AggregateRoot
    {
        public long OrganizationId { get; private set; }
        public string? Sku { get; private set; }
        public string Name { get; private set; } = null!;
        public string? PageUrl { get; private set; }
        public ProductStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Organization? Organization { get; private set; }

        private Product() : base(0) { }

        private Product(long organizationId, string? sku, string name, string? pageUrl) : base(0)
        {
            OrganizationId = organizationId;
            Sku = sku;
            Name = name;
            PageUrl = pageUrl;
            Status = ProductStatus.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<Product> Create(long organizationId, string name, string? sku, string? pageUrl)
        {
            if (organizationId <= 0)
                return Result.Failure<Product>(Error.Validation("Product.OrganizationRequired", "A organização é obrigatória."));
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Product>(Error.Validation("Product.NameRequired", "O nome do produto é obrigatório."));

            return Result.Success(new Product(organizationId, sku?.Trim(), name.Trim(), pageUrl));
        }

        public void Update(string name, string? sku, string? pageUrl)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do produto é obrigatório.", nameof(name));

            Name = name.Trim();
            Sku = sku?.Trim();
            PageUrl = pageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            Status = ProductStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            Status = ProductStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
