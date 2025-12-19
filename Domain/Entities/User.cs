namespace Domain.Entities;

public class User : BaseEntity{
    public string Username {get; set;} = null!;
    public string PasswordHash {get; set;} = null!;
    public string Role {get; set;} = "User";
    public ICollection<Review> Reviews {get; set;} = new List<Review>();}