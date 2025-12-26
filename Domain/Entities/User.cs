namespace Domain.Entities;

public class User : BaseEntity{
    public string Username {get; set;} = null!;
    public string Email { get; set; } = null!;
    public ICollection<Review> Reviews {get; set;} = new List<Review>();}