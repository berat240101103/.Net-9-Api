using Application.DTOs.Reviews;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

public class ReviewService : IReviewService{
    private readonly AppDbContext _context;
    public ReviewService(AppDbContext context){_context = context;}
    public async Task<List<ReviewResponseDto>> GetByProductIdAsync(int productId){
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .Select(r => new ReviewResponseDto{
                Id = r.Id,
                Comment = r.Comment,
                Rating = r.Rating,
                Username = r.User.Username
            }).ToListAsync();}
    public async Task<ReviewResponseDto> CreateAsync(int productId, ReviewCreateDto dto){
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            throw new Exception("Product not found");
        var review = new Review{
            ProductId = productId,
            UserId = dto.UserId,
            Comment = dto.Comment,
            Rating = dto.Rating
        };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(dto.UserId);
        return new ReviewResponseDto{
            Id = review.Id,
            Comment = review.Comment,
            Rating = review.Rating,
            Username = user!.Username
        };}}
