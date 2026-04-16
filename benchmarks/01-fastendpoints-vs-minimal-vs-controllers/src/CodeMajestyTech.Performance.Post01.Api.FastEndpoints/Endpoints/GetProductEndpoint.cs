using CodeMajestyTech.Performance.Post01.Shared;
using FastEndpoints;

namespace CodeMajestyTech.Performance.Post01.Api.FastEndpoints.Endpoints;

public sealed class GetProductRequest
{
    public int Id { get; set; }
}

public sealed class GetProductEndpoint(ProductService service) : Endpoint<GetProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Get("/products/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var product = await service.GetByIdAsync(req.Id, ct);
        if (product is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}