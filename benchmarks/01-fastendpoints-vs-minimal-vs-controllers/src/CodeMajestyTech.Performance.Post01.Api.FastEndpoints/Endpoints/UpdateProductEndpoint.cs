using CodeMajestyTech.Performance.Post01.Shared;
using FastEndpoints;
using FluentValidation;

namespace CodeMajestyTech.Performance.Post01.Api.FastEndpoints.Endpoints;

public sealed class UpdateProductEndpointRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public sealed class UpdateProductValidator : Validator<UpdateProductEndpointRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductEndpoint(ProductService service)
    : Endpoint<UpdateProductEndpointRequest, ProductResponse>
{
    public override void Configure()
    {
        Put("/products/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateProductEndpointRequest req, CancellationToken ct)
    {
        var dto = new UpdateProductRequest(req.Name, req.Description, req.Price, req.StockQuantity);
        var product = await service.UpdateAsync(req.Id, dto, ct);
        if (product is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}