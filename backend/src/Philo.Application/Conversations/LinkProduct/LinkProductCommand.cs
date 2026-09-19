using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.LinkProduct
{
    /// <summary>Divulgação = conversar sobre produtos do catálogo; o nome é preservado no momento do vínculo.</summary>
    public sealed record LinkProductCommand(long OrganizationId, long ConversationId, long ProductId) : IRequest<Result>;

    public sealed class LinkProductValidator : AbstractValidator<LinkProductCommand>
    {
        public LinkProductValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.ProductId).GreaterThan(0);
        }
    }

    public sealed class LinkProductHandler : IRequestHandler<LinkProductCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IProductRepository _products;
        private readonly IConversationProductRepository _links;

        public LinkProductHandler(IConversationRepository conversations, IProductRepository products, IConversationProductRepository links)
        {
            _conversations = conversations;
            _products = products;
            _links = links;
        }

        public async Task<Result> Handle(LinkProductCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, cancellationToken);
            if (conversation is null)
                return Result.Failure(DomainErrors.Conversation.NotFound);

            var product = await _products.GetByIdForOrganizationAsync(request.OrganizationId, request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(DomainErrors.Product.NotFound);

            var linkResult = ConversationProduct.Create(request.OrganizationId, request.ConversationId, request.ProductId, product.Name);
            if (linkResult.IsFailure)
                return Result.Failure(linkResult.Error);

            await _links.AddAsync(linkResult.Value, cancellationToken);
            return Result.Success();
        }
    }
}
