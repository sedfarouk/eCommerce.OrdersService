namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderUpdateRequest(Guid OrderId, Guid UserId, DateTime OrderDate, List<OrderItemAddRequest> OrderItems)
{
    public OrderUpdateRequest(): this(default, default, default, default)
    {
        
    }
};