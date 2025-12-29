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
using Api.Common.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Serilog;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"]?? throw new InvalidOperationException("Jwt:Key missing");
var key = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>{
        options.TokenValidationParameters = new TokenValidationParameters{
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role};
        options.Events = new JwtBearerEvents{
            OnChallenge = context =>{
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailResponse("Unauthorized"));},
            OnForbidden = context =>{
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(
                    ApiResponse<string>.FailResponse("Forbidden"));}};});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Infrastructure")));
builder.Services.AddSwaggerGen(c =>{c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new(){{
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
builder.Host.UseSerilog();
var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/categories", async (AppDbContext context) =>{
    var categories = await context.Categories.AsNoTracking().Select(c => new CategoryResponseDto{
            Id = c.Id,
            Name = c.Name}).ToListAsync();
    return Results.Ok(ApiResponse<List<CategoryResponseDto>>.SuccessResponse(categories, "Categories listed"));});
app.MapPost("/categories", [Authorize(Roles = "Admin")] async (CategoryCreateDto dto, AppDbContext context) =>{
    var category = new Category{Name = dto.Name, CreatedAt = DateTime.UtcNow};
    context.Categories.Add(category);
    await context.SaveChangesAsync();
    return Results.Created($"/categories/{category.Id}", ApiResponse<CategoryResponseDto>.SuccessResponse(
        new CategoryResponseDto{Id = category.Id,Name = category.Name},"Category created"));});
app.MapGet("/products", async (IProductService service) =>{
    var products = await service.GetAllAsync();
    return Results.Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products listed"));});
app.MapGet("/products/{id:int}", async (int id, IProductService service) =>{
    var product = await service.GetByIdAsync(id);
    return product == null
        ? Results.NotFound(ApiResponse<object>.FailResponse("Product not found"))
        : Results.Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product fetched"));});
app.MapPost("/products", [Authorize(Roles = "Admin")] async (ProductCreateDto dto, IProductService service) =>{
    var product = await service.CreateAsync(dto);
    return Results.Created($"/products/{product.Id}", ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product created"));});
app.MapPut("/products/{id:int}", [Authorize(Roles = "Admin")] async (int id, ProductUpdateDto dto, IProductService service) =>{
    await service.UpdateAsync(id, dto);
    return Results.Ok(ApiResponse<object>.SpecialSuccessResponse("Product updated"));});
app.MapDelete("/products/{id:int}", [Authorize(Roles = "Admin")] async (int id, IProductService service) =>{
    await service.DeleteAsync(id);
    return Results.Ok(ApiResponse<object>.SpecialSuccessResponse("Product deleted"));});
app.MapGet("/products/{id:int}/reviews", async (int id, IReviewService service) =>{
    var reviews = await service.GetByProductIdAsync(id);
    return Results.Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(reviews, "Reviews listed"));});
app.MapPost("/products/{id:int}/reviews", [Authorize] async (int id, ReviewCreateDto dto, HttpContext http, IReviewService service) =>{
    var userId = int.Parse(http.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var review = await service.CreateAsync(id, dto, userId);
    return Results.Created($"/products/{id}/reviews/",ApiResponse<ReviewResponseDto>.SuccessResponse(review, "Review created"));});
app.MapGet("/users", async (IUserService service) =>{
    var users = await service.GetAllAsync();
    return Results.Ok(ApiResponse<List<UserResponseDto>>.SuccessResponse(users, "Users listed"));});
app.MapGet("/users/{id:int}", async (int id, IUserService service) =>{
    var user = await service.GetByIdAsync(id);
    return user == null
        ? Results.NotFound(ApiResponse<object>.FailResponse("User not found"))
        : Results.Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User fetched"));});
app.MapPost("/users", async (UserCreateDto dto, IUserService service) =>{
    var user = await service.CreateAsync(dto);
    return Results.Created($"/users/{user.Id}",ApiResponse<UserResponseDto>.SuccessResponse(user, "User created"));});
app.MapPut("/users/{id:int}", [Authorize(Roles = "Admin")] async (int id, UserUpdateDto dto, IUserService service) =>{
    await service.UpdateAsync(id, dto);
    return Results.Ok(ApiResponse<object>.SpecialSuccessResponse("User updated"));});
app.MapDelete("/users/{id:int}", [Authorize(Roles = "Admin")] async (int id, IUserService service) =>{
    await service.DeleteAsync(id);
    return Results.Ok(ApiResponse<object>.SpecialSuccessResponse("User deleted"));});
app.MapPut("/users/{id:int}/role", [Authorize(Roles = "Admin")] async (int id, UserRoleUpdateDto dto, IUserService service) =>{
    await service.UpdateRoleAsync(id, dto.Role);
    return Results.Ok(ApiResponse<object>.SpecialSuccessResponse("User role updated"));});
app.MapPost("/login", async (LoginDto dto, IAuthService authService) =>{
    var token = await authService.AuthenticateAsync(dto);
    if (token == null){
        return Results.Json(
            ApiResponse<string>.FailResponse("Invalid credentials"),
            statusCode: StatusCodes.Status401Unauthorized);}
    return Results.Ok(
        ApiResponse<string>.SuccessResponse(token, "Login successful"));});
app.Run();
