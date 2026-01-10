using Application.DTOs.Reviews;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services;

public class ReviewService : IReviewService{
    private readonly AppDbContext _context;
    public ReviewService(AppDbContext context){_context = context;}
    public async Task<List<ReviewResponseDto>> GetByProductIdAsync(int productId){
    return await _context.Reviews
        .Include(r => r.User)
        .Where(r =>
            r.ProductId == productId &&
            !r.IsDeleted &&
            !r.User.IsDeleted)
        .Select(r => new ReviewResponseDto{
            Comment = r.Comment,
            Rating = r.Rating,
            Username = r.User.Username
            }).ToListAsync();}
public async Task<ReviewResponseDto> CreateAsync(int productId, ReviewCreateDto dto, int userId){
    if (dto.Rating < 1 || dto.Rating > 5)
        throw new ValidationException("Rating must be between 1 and 5");
    if (string.IsNullOrWhiteSpace(dto.Comment))
        throw new ValidationException("Comment cannot be empty");
    var exists = await _context.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId);
    if (exists)
        throw new ValidationException("You have already reviewed this product");
    var productExists = await _context.Products.AnyAsync(p => p.Id == productId && !p.IsDeleted);
    if (!productExists)
        throw new ValidationException("Product not found");
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    if (user == null)
        throw new ValidationException("User not found");
    var review = new Review{
        ProductId = productId,
        UserId = userId,
        Comment = dto.Comment,
        Rating = dto.Rating};
    _context.Reviews.Add(review);
    await _context.SaveChangesAsync();
    return new ReviewResponseDto{
        Comment = review.Comment,
        Rating = review.Rating,
        Username = user.Username};}}
