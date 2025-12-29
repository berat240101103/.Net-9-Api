namespace Application.DTOs.Products;
    public class ProductResponseDto{
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }}