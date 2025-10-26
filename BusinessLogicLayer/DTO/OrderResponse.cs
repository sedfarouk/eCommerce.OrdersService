namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderId, Guid UserId, string? PersonName, string? Email, decimal TotalBill, DateTime OrderDate, List<OrderItemResponse> OrderItems)
{
    public OrderResponse(): this(default, default, default, default, default, default, default)
    {
        
    }
};