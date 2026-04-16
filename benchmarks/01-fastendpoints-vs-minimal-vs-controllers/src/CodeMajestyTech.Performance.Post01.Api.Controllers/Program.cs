using CodeMajestyTech.Performance.Post01.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BenchmarkDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Benchmark")));
builder.Services.AddScoped<ProductService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;