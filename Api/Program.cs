using Application.DTOs.Products;
using Application.DTOs.Categories;
using Application.DTOs.Users;
using Application.DTOs.Reviews;
using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Api.Common.Responses;
using Api.Common.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection"),b => b.MigrationsAssembly("Infrastructure")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/test-error", () =>
{
    throw new Exception("TEST LOGGING EXCEPTION");
});
app.MapGet("/categories", async (AppDbContext context) =>{
    var categories = await context.Categories
        .Select(c => new CategoryResponseDto{
            Id = c.Id,
            Name = c.Name
        })
        .ToListAsync();
    return Results.Ok(
        ApiResponse<List<CategoryResponseDto>>
            .SuccessResponse(categories, "Categories listed"));});
app.MapGet("/products", async (IProductService service) =>{
    var data = await service.GetAllAsync();
    return Results.Ok(ApiResponse<List<ProductResponseDto>>
        .SuccessResponse(data, "Products listed"));});
app.MapGet("/products/{id:int}", async (int id, IProductService service) =>{
    var product = await service.GetByIdAsync(id);
    if (product == null)
        return Results.BadRequest(ApiResponse<ProductResponseDto>.FailResponse("Product not found"));
    return Results.Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product));});
app.MapPost("/categories", async (CategoryCreateDto dto,AppDbContext context) =>{
    var category = new Category{
        Name = dto.Name,
        CreatedAt = DateTime.UtcNow};
    context.Categories.Add(category);
    await context.SaveChangesAsync();
    return Results.Created(
        $"/categories/{category.Id}",
        ApiResponse<CategoryResponseDto>.SuccessResponse(
            new CategoryResponseDto{
                Id = category.Id,
                Name = category.Name
            },"Category created"));});
app.MapPost("/products", async (ProductCreateDto dto, IProductService service) =>{
    var product = await service.CreateAsync(dto);
    return Results.Created($"/products/{product.Id}",
        ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product created"));});
app.MapPut("/products/{id:int}", async (int id, ProductUpdateDto dto, IProductService service) =>{
    var updated = await service.UpdateAsync(id, dto);
    if (!updated)
        return Results.NotFound(ApiResponse<object>.FailResponse("Product not found"));
    return Results.Ok(ApiResponse<object?>.SuccessResponse(null, "Product Updated"));});
app.MapDelete("/products/{id:int}", async (int id, IProductService service) =>{
    var deleted = await service.DeleteAsync(id);
    if (!deleted)
        return Results.NotFound(ApiResponse<object>.FailResponse("Product not found"));
    return Results.NoContent();});
app.MapGet("/users", async (IUserService service) =>{
    var users = await service.GetAllAsync();
    return Results.Ok(ApiResponse<List<UserResponseDto>>.SuccessResponse(users));});
app.MapGet("/users/{id:int}", async (int id, IUserService service) =>{
    var user = await service.GetByIdAsync(id);
    return user == null
        ? Results.NotFound(ApiResponse<object>.FailResponse("User not found"))
        : Results.Ok(ApiResponse<UserResponseDto>.SuccessResponse(user));});
app.MapPost("/users", async (UserCreateDto dto, IUserService service) =>{
    var user = await service.CreateAsync(dto);
    return Results.Created($"/users/{user.Id}",
        ApiResponse<UserResponseDto>.SuccessResponse(user, "User created"));});
app.MapPut("/users/{id:int}", async (int id, UserUpdateDto dto, IUserService service) =>{
    var result = await service.UpdateAsync(id, dto);
    return result
        ? Results.NoContent()
        : Results.NotFound(ApiResponse<object>.FailResponse("User not found"));});
app.MapDelete("/users/{id:int}", async (int id, IUserService service) =>{
    var result = await service.DeleteAsync(id);
    return result
        ? Results.NoContent()
        : Results.NotFound(ApiResponse<object>.FailResponse("User not found"));});
app.MapGet("/products/{id:int}/reviews", async (int id,IReviewService service) =>{
    var reviews = await service.GetByProductIdAsync(id);
    return Results.Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(reviews));});
app.MapPost("/products/{id:int}/reviews", async (int id,ReviewCreateDto dto,IReviewService service) =>{
    var review = await service.CreateAsync(id, dto);
    return Results.Created(
        $"/products/{id}/reviews/{review.Id}",
        ApiResponse<ReviewResponseDto>.SuccessResponse(review)
    );});
app.Run();