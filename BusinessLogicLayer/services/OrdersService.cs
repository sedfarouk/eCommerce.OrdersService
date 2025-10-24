using FluentValidation.Results;
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    
    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator,IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
    }
    
    public Task<List<OrderResponse?>> GetOrders()
    {
        throw new NotImplementedException();
    }

    public Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }

        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);

        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));

            throw new ArgumentException(errors);
        }
        
        // Validate order items using Fluent Validation
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ",
                    orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));

                throw new Exception(errors);
            }
        }
        
        // TO DO: Add logic for checking if User od exists in Users microservice table
        
        
        // Convert data from OrderAddRequest to Order
        Order orderInput = _mapper.Map<Order>(orderAddRequest);
        
        // Generate values
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);
        
        // Invoke repository
        Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

        if (addedOrder == null)
        {
            return null;
        }

        return _mapper.Map<OrderResponse>(addedOrder);
    }

    public Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteOrder(Guid orderId)
    {
        throw new NotImplementedException();
    }
}