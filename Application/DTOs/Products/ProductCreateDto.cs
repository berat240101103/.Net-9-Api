namespace Application.DTOs.Products;
    public class ProductCreateDto{
        public string Name {get; set;} = null!;
        public decimal Price {get; set;}
        public int CategoryId {get; set;}}