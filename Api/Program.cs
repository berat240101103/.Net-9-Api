using Application.DTOs.Products;
using Application.DTOs.Categories;
using Application.DTOs.Users;
using Application.DTOs.Reviews;
using Application.DTOs.Logins;
using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Api.Common.Responses;
using Api.Common.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not found in configuration.");
var key = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;})
.AddJwtBearer(options => {options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))};});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Infrastructure")));
builder.Services.AddSwaggerGen(c =>{c.SwaggerDoc("v1", new() { Title = "Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header. Example: 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{{
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
                Reference = new Microsoft.OpenApi.Models.OpenApiReference{
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }},
            Array.Empty<string>()}});});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Host.UseSerilog();
var app = builder.Build();
//app.UseMiddleware<ExceptionMiddleware>();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

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
app.MapGet("/products", async (IProductService service, ILogger<Program> logger) =>{
    logger.LogInformation("/products endpoint called.");
    var data = await service.GetAllAsync();
    logger.LogInformation("{Count} product returned.", data.Count);
    return Results.Ok(ApiResponse<List<ProductResponseDto>>
        .SuccessResponse(data, "Products listed"));});
app.MapGet("/products/{id:int}", async (int id, IProductService service) =>{
    var product = await service.GetByIdAsync(id);
    if (product == null)
        return Results.NotFound(ApiResponse<object>.FailResponse("Product not found"));
    return Results.Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product));});
app.MapPost("/categories", [Authorize(Roles = "Admin")] async (CategoryCreateDto dto,AppDbContext context) =>{
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
app.MapPost("/products", [Authorize(Roles = "Admin")] async (ProductCreateDto dto, IProductService service) =>{
    var product = await service.CreateAsync(dto);
    return Results.Created($"/products/{product.Id}",
        ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product created"));});
app.MapPut("/products/{id:int}", [Authorize(Roles = "Admin")] async (int id, ProductUpdateDto dto, IProductService service) =>{
    var updated = await service.UpdateAsync(id, dto);
    if (!updated)
        return Results.NotFound(ApiResponse<object>.FailResponse("Product not found"));
    return Results.Ok(ApiResponse<object?>.SuccessResponse(null, "Product Updated"));});
app.MapDelete("/products/{id:int}", [Authorize(Roles = "Admin")] async (int id, IProductService service) =>{
    var success = await service.DeleteAsync(id);
    if (!success)
        return Results.NotFound(ApiResponse<string>.FailResponse("Product not found"));
    return Results.Ok(ApiResponse<string?>.SuccessResponse(null, "Product soft-deleted"));});
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
app.MapPut("/users/{id:int}", [Authorize(Roles = "Admin")] async (int id, UserUpdateDto dto, IUserService service) =>{
    var result = await service.UpdateAsync(id, dto);
    return result
        ? Results.NoContent()
        : Results.NotFound(ApiResponse<object>.FailResponse("User not found"));});
app.MapDelete("/users/{id:int}", [Authorize(Roles = "Admin")] async (int id, IUserService service) =>{
    var result = await service.DeleteAsync(id);
    return result
        ? Results.NoContent()
        : Results.NotFound(ApiResponse<object>.FailResponse("User not found"));});
app.MapGet("/products/{id:int}/reviews", async (int id,IReviewService service) =>{
    var reviews = await service.GetByProductIdAsync(id);
    return Results.Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(reviews));});
app.MapPost("/products/{id:int}/reviews", [Authorize(Roles = "Admin")] async (int id,ReviewCreateDto dto,IReviewService service) =>{
    var review = await service.CreateAsync(id, dto);
    return Results.Created(
        $"/products/{id}/reviews/{review.Id}",
        ApiResponse<ReviewResponseDto>.SuccessResponse(review)
    );});
app.MapPost("/login", async (LoginDto login, IAuthService authService) => {
    var token = await authService.AuthenticateAsync(login);
    if (token == null) return Results.Unauthorized();
    return Results.Ok(new { token });});
app.MapGet("/products/all", [Authorize] async (IProductService service) => {
    var data = await service.GetAllAsync();
    return Results.Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(data));});
app.MapPut("/users/{id}/role", [Authorize(Roles = "Admin")] async (int id, UserRoleUpdateDto dto, IUserService service) =>{
    var success = await service.UpdateRoleAsync(id, dto.Role);
    return success? Results.Ok(): Results.NotFound();});
app.Run();