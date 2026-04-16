using CodeMajestyTech.Performance.Post01.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BenchmarkDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Benchmark")));
builder.Services.AddScoped<ProductService>();
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseFastEndpoints();

app.Run();

public partial class Program;