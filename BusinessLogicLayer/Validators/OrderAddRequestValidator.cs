using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator()
    {
        RuleFor(temp => temp.UserId)
            .NotEmpty()
            .WithErrorCode("User id can't be blank");
        
        RuleFor(temp => temp.OrderDate)
            .NotEmpty()
            .WithErrorCode("Order date can't be blank");
        
        RuleFor(temp => temp.OrderItems)
            .NotEmpty()
            .WithErrorCode("Order items can't be blank");
    }
}