using Application.DTOs.Products;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services;

public class ProductService : IProductService{
    private readonly ILogger<ProductService> _logger;
    private readonly AppDbContext _context;
    public ProductService(AppDbContext context, ILogger<ProductService> logger){
        _context = context;
        _logger = logger;}
    public async Task<List<ProductResponseDto>> GetAllAsync(){
        var products = await _context.Products
            .Where(p => !p.IsDeleted)
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();
        return products.Select(p => new ProductResponseDto{
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
            }).ToList();}
    public async Task<ProductResponseDto?> GetByIdAsync(int id){
        var product = await _context.Products
            .Where(p => p.Id == id && !p.IsDeleted)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (product == null){
            _logger.LogWarning("ProductId={Id} not found.", id);
            return null;}
        return new ProductResponseDto{
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt};}
    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto){
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Product name is required");
        if (dto.Price <= 0)
            throw new ValidationException("Price must be greater than zero");
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            throw new ValidationException("Category not found");
        var productExists = await _context.Products.AnyAsync(p => p.Name == dto.Name && !p.IsDeleted);
        if (productExists)
            throw new ValidationException("Product already exists");
        var product = new Product{
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow};
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return new ProductResponseDto{
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt};}
    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto){
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null)
            throw new KeyNotFoundException("Product not found");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Product name is required");
        if (dto.Price <= 0)
            throw new ValidationException("Price must be greater than zero");
        product.Name = dto.Name;
        product.Price = dto.Price;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}
    public async Task<bool> DeleteAsync(int id){
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new KeyNotFoundException("Product not found");
        if (product.IsDeleted)
            throw new InvalidOperationException("Product already deleted");
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}}
