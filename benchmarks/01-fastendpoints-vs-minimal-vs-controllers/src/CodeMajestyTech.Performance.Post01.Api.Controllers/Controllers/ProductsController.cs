using CodeMajestyTech.Performance.Post01.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CodeMajestyTech.Performance.Post01.Api.Controllers.Controllers;

[ApiController]
[Route("products")]
public sealed class ProductsController(
    ProductService service,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await service.GetByIdAsync(id, ct);
        return product is not null ? Ok(product) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(int page = 1, int size = 20, CancellationToken ct = default)
    {
        var result = await service.GetPagedAsync(page, size, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var product = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var product = await service.UpdateAsync(id, request, ct);
        return product is not null ? Ok(product) : NotFound();
    }
}