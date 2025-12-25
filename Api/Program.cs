using Api.DTOs;
using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Api.Common;
using Api.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IProductService, ProductService>();
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/products", async ([FromServices] ProductService service) =>{
    var products = await service.GetAllAsync();
    var dto = products.Select(p => new ProductResponseDTO{
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        CategoryName = p.Category.Name});
    return Results.Ok(ApiResponse<object>.SuccessResponse(dto, "Products listed"));});
app.MapGet("/products/{id:int}", async ([FromRoute] int id, [FromServices] ProductService service) =>{
    var product = await service.GetByIdAsync(id);
    if (product == null)
        return Results.NotFound(ApiResponse<string>.FailResponse("Product not found"));
    var dto = new ProductResponseDTO{
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        CategoryName = product.Category.Name};
    return Results.Ok(ApiResponse<ProductResponseDTO>.SuccessResponse(dto));});
app.MapPost("/products", async ([FromBody] ProductCreateDTO dto,[FromServices] IProductService service) =>{
    var product = new Product{
        Name = dto.Name,
        Price = dto.Price,
        CategoryId = dto.CategoryId};
    var created = await service.CreateAsync(product);
    return Results.Created(
        $"/products/{created.Id}",
        ApiResponse<Product>.SuccessResponse(created, "Product created"));});
app.MapPut("/products/{id:int}", async ([FromRoute] int id,[FromBody] ProductUpdateDTO dto,[FromServices] IProductService service) =>{
    var product = new Product{
        Id = id,
        Name = dto.Name,
        Price = dto.Price,
        CategoryId = dto.CategoryId};
    var success = await service.UpdateAsync(product);
    if (!success)
        return Results.NotFound(
            ApiResponse<string>.FailResponse("Product not found"));
    return Results.Ok(
        ApiResponse<string?>.SuccessResponse(null, "Product updated"));});
app.MapDelete("/products/{id:int}", async ([FromRoute] int id,[FromServices] IProductService service) =>{
    var success = await service.DeleteAsync(id);
    if (!success)
        return Results.NotFound(ApiResponse<string>.FailResponse("Product not found"));
    return Results.NoContent();});
app.Run();