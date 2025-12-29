namespace Application.DTOs.Reviews;
    public class ReviewCreateDto{
        public string Comment { get; set; } = null!;
        public int Rating { get; set; }
        public int UserId { get; set; }}