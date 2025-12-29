using Application.DTOs.Reviews;

namespace Application.Interfaces;

public interface IReviewService{
    Task<List<ReviewResponseDto>> GetByProductIdAsync(int productId);
    Task<ReviewResponseDto> CreateAsync(int productId, ReviewCreateDto dto);}
