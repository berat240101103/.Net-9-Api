using Application.DTOs.Products;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Services;

public class ProductService : IProductService{
    private readonly AppDbContext _context;
    public ProductService(AppDbContext context){_context = context;}
    public async Task<List<ProductResponseDto>> GetAllAsync(){
        return await _context.Products
            .Select(p => new ProductResponseDto{
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();}
     public async Task<ProductResponseDto?> GetByIdAsync(int id){
        return await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto{
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();}
    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto){
        var product = new Product{
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return new ProductResponseDto{
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt
        };}
    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto){
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        product.Name = dto.Name;
        product.Price = dto.Price;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}
    public async Task<bool> DeleteAsync(int id){
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;}}