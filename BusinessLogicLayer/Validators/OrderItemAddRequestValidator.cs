using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderItemAddRequestValidator : AbstractValidator<OrderItem>
{
    public OrderItemAddRequestValidator()
    {
        RuleFor(product => product.ProductId)
            .NotEmpty()
            .WithErrorCode("Product Id can not be blank");

        RuleFor(product => product.UnitPrice)
            .NotEmpty()
            .WithErrorCode("Product Id can not be blank")
            .GreaterThan(0).WithErrorCode("Unit price can't be less than or equal to 0");
        
        RuleFor(product => product.Quantity)
            .NotEmpty()
            .WithErrorCode("Quantity can not be blank")
            .GreaterThan(0).WithErrorCode("Quantity can not be less than or equal to 0");
    }
}