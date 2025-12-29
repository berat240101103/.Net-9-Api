namespace Application.DTOs.Reviews;
    public class ReviewResponseDto{
        public int Id { get; set; }
        public string Comment { get; set; } = null!;
        public int Rating { get; set; }
        public string Username { get; set; } = null!;}