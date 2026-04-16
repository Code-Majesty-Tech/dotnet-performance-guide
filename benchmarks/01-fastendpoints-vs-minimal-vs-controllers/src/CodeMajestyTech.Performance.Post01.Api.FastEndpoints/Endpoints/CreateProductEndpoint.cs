using CodeMajestyTech.Performance.Post01.Shared;
using FastEndpoints;
using FluentValidation;

namespace CodeMajestyTech.Performance.Post01.Api.FastEndpoints.Endpoints;

public sealed class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

public sealed class CreateProductEndpoint(ProductService service) : Endpoint<CreateProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = await service.CreateAsync(req, ct);
        await SendCreatedAtAsync<GetProductEndpoint>(new { product.Id }, product, cancellation: ct);
    }
}