using CodeMajestyTech.Performance.Post01.Shared;
using FastEndpoints;

namespace CodeMajestyTech.Performance.Post01.Api.FastEndpoints.Endpoints;

public sealed class GetPagedProductsRequest
{
    [QueryParam] public int Page { get; set; } = 1;
    [QueryParam] public int Size { get; set; } = 20;
}

public sealed class GetPagedProductsEndpoint(ProductService service)
    : Endpoint<GetPagedProductsRequest, PagedResponse<ProductResponse>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPagedProductsRequest req, CancellationToken ct)
    {
        var result = await service.GetPagedAsync(req.Page, req.Size, ct);
        await SendAsync(result, cancellation: ct);
    }
}