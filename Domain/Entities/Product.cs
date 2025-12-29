namespace Domain.Entities;

public class Product : BaseEntity{
    public string Name {get; set;} = null!;
    public decimal Price {get; set;}
    public int CategoryId {get; set;}
    public Category Category {get; set;} = null!;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();}