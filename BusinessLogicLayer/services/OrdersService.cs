using FluentValidation.Results;
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
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
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    
    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator,IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }
    
    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDto =
                    await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductId);

                if (productDto == null)
                {
                    continue;
                }
                
                _mapper.Map<ProductDTO, OrderItemResponse>(productDto, orderItemResponse);
            } 
        }

        foreach (OrderResponse orderResponse in orderResponses)
        {
            UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserId(orderResponse.UserId);

            if (userDto is null)
            {
                continue;
            }

            _mapper.Map<UserDTO, OrderResponse>(userDto, orderResponse);
        }

        return orderResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);
        
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDto =
                    await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductId);

                if (productDto == null)
                {
                    continue;
                }
                
                _mapper.Map<ProductDTO, OrderItemResponse>(productDto, orderItemResponse);
            } 
        } 
        
        foreach (OrderResponse orderResponse in orderResponses)
        {
            UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserId(orderResponse.UserId);

            if (userDto is null)
            {
                continue;
            }

            _mapper.Map<UserDTO, OrderResponse>(userDto, orderResponse);
        }

        return orderResponses.ToList();
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);

        if (order == null)
        {
            return null;
        }
        
        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);

        if (orderResponse == null)
        {
            return null;
        }

        foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
        {
            ProductDTO? productDto =
                await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductId);

            if (productDto == null)
            {
                continue;
            }
            
            _mapper.Map<ProductDTO, OrderItemResponse>(productDto, orderItemResponse);
        }
        

        UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserId(orderResponse.UserId);

        if (userDto is null)
        {
            return null;
        }

        _mapper.Map<UserDTO, OrderResponse>(userDto, orderResponse);

        return orderResponse;
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

        List<ProductDTO?> productDtos = new List<ProductDTO?>();
        
        // Validate order items using Fluent Validation
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ",
                    orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));

                throw new ArgumentException(errors);
            }
            
            ProductDTO? productDto = await _productsMicroserviceClient.GetProductByProductId(orderItemAddRequest.ProductId);

            if (productDto is null)
            {
                throw new ArgumentException($"Invalid product id - {orderItemAddRequest.ProductId}");
            }
            
            productDtos.Add(productDto);
        }
        
        // TO DO: Add logic for checking if User id exists in Users microservice table
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserId(orderAddRequest.UserId);

        if (user == null)
        {
            throw new ArgumentException("Invalid user id");
        }
        
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

        OrderResponse? addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);

        if (addedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                ProductDTO? productDto =
                    productDtos.FirstOrDefault(product => product.ProductId == orderItemResponse.ProductId);

                if (productDto == null)
                {
                    continue;
                }
            
                _mapper.Map<ProductDTO, OrderItemResponse>(productDto, orderItemResponse);
            }

            UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserId(addedOrderResponse.UserId);

            if (userDto is null)
            {
                return addedOrderResponse;
            }

            _mapper.Map<UserDTO, OrderResponse>(userDto, addedOrderResponse);
        }

        return addedOrderResponse;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }

        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));

            throw new ArgumentException(errors);
        }

        List<ProductDTO?> productDtos = new List<ProductDTO?>();
        
        // Validate order items using Fluent Validation
        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ",
                    orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));

                throw new Exception(errors);
            }
            
            ProductDTO? productDto = await _productsMicroserviceClient.GetProductByProductId(orderItemUpdateRequest.ProductId);

            if (productDto is null)
            {
                throw new ArgumentException($"Invalid product id - {orderItemUpdateRequest.ProductId}");
            }

            productDtos.Add(productDto);
        }
        
        // TO DO: Add logic for checking if User id exists in Users microservice table
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserId(orderUpdateRequest.UserId);

        if (user == null)
        {
            throw new ArgumentException("Invalid user id");
        }
        
        // Convert data from OrderAddRequest to Order
        Order orderInput = _mapper.Map<Order>(orderUpdateRequest);
        
        // Generate values
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);
        
        // Invoke repository
        Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);

        if (updatedOrderResponse == null)
        {
            return null;
        }
        
        foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
        {
            ProductDTO? productDto =
                await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductId);

            if (productDto == null)
            {
                continue;
            }
            
            _mapper.Map<ProductDTO, OrderItemResponse>(productDto, orderItemResponse);
        }
        
        UserDTO? userDto = await _usersMicroserviceClient.GetUserByUserId(updatedOrderResponse.UserId);

        if (userDto is null)
        {
            return updatedOrderResponse;
        }

        _mapper.Map<UserDTO, OrderResponse>(userDto, updatedOrderResponse);
        
        return updatedOrderResponse;
    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, orderId);

        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
        {
            return false;
        }

        bool isDeleted = await _ordersRepository.DeleteOrder(orderId);
        return isDeleted;
    }
}