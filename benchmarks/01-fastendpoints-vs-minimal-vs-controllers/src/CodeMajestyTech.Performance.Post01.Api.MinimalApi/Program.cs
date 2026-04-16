using CodeMajestyTech.Performance.Post01.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BenchmarkDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Benchmark")));
builder.Services.AddScoped<ProductService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.MapGet("/products/{id:int}", async (int id, ProductService service, CancellationToken ct) =>
{
    var product = await service.GetByIdAsync(id, ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapGet("/products", async (ProductService service, CancellationToken ct, int page = 1, int size = 20) =>
{
    var result = await service.GetPagedAsync(page, size, ct);
    return Results.Ok(result);
});

app.MapPost("/products", async (
    CreateProductRequest request,
    IValidator<CreateProductRequest> validator,
    ProductService service,
    CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var product = await service.CreateAsync(request, ct);
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:int}", async (
    int id,
    UpdateProductRequest request,
    IValidator<UpdateProductRequest> validator,
    ProductService service,
    CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var product = await service.UpdateAsync(id, request, ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.Run();

public partial class Program;