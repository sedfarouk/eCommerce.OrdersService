namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record ProductDTO(Guid ProductId, string? ProductName, string? Category, int UnitPrice, int QuantityInStock)
{
    public ProductDTO() : this(default, default, default, default, default)
    {
        
    }
};