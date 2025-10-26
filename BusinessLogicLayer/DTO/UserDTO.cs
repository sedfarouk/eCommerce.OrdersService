namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record UserDTO(Guid UserId, string? PersonName, string? Email, string Gender)
{
    public UserDTO() : this(default, default, default, default)
    {
        
    }
};