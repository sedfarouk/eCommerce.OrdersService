namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderItemUpdateRequest(Guid ProductId, decimal UnitPrice, int Quantity)
{
    public OrderItemUpdateRequest(): this(default, default, default)
    {
        
    }
}