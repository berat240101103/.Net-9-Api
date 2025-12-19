namespace Domain.Entities;

public class Review : BaseEntity{
    public string Comment {get; set;} = null!;
    public int Rating {get; set;}
    public int ProductId {get; set;}
    public Product Product {get; set;} = null!;
    public int UserId {get; set;}
    public User User {get; set;} = null!;}