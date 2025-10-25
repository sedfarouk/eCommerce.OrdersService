using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }
    
    // GET: /api/orders
    [HttpGet]
    public async Task<IEnumerable<OrderResponse?>> Get()
    {
        List<OrderResponse?> orders = await _ordersService.GetOrders();

        return orders;
    }
    
    // GET: /api/orders/search/order_id/{orderId}
    [HttpGet("search/orderId/{orderId}")]
    public async Task<OrderResponse?> GetOrderByOrderId(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, orderId);

        OrderResponse? orderResponse = await _ordersService.GetOrderByCondition(filter);

        return orderResponse;
    }
    
    // GET: /api/orders/search/product_id/{productId}
    [HttpGet("search/productId/{productId}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductId(Guid productId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems,
            Builders<OrderItem>.Filter.Eq(tempProduct => tempProduct.ProductId, productId));
        
        List<OrderResponse?> orderResponse = await _ordersService.GetOrdersByCondition(filter);
        
        return orderResponse;   
    }
    
    // GET: /api/orders/search/order_date/{orderDate}
    [HttpGet("search/orderDate/{orderDate}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime dateTime)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyy-MM-dd"), dateTime.ToString("yyy-MM-dd"));
        
        List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
        
        return orders;   
    }
    
    // GET: /api/orders/search/user_id/{userId}
    [HttpGet("search/userId/{userId}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserId(Guid userId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserId, userId);
        
        List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
        
        return orders;   
    }
    
    // POST api/orders
    [HttpPost]
    public async Task<IActionResult> Post(OrderAddRequest? orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        OrderResponse? orderResponse = await _ordersService.AddOrder(orderAddRequest);

        if (orderResponse == null)
        {
            return Problem("Error creating order");
        }

        return Created($"api/orders/search/orderId/{orderResponse?.OrderId}", orderResponse);
    }
    
    // PUT api/orders
    [HttpPut("{orderId}")]
    public async Task<IActionResult> Put(Guid orderId, OrderUpdateRequest? orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        if (orderId != orderUpdateRequest.OrderId)
        {
            return BadRequest("Order id in the url doesn't match the url of the order update item");
        }

        OrderResponse? orderResponse = await _ordersService.UpdateOrder(orderUpdateRequest);

        if (orderResponse == null)
        {
            return Problem("Error updating order");
        }

        return Ok(orderResponse);
    }
    
    // POST api/orders
    [HttpDelete("{orderId}")]
    public async Task<IActionResult> Delete(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("Invalid order id");
        }

        bool isDeleted = await _ordersService.DeleteOrder(orderId);

        if (!isDeleted)
        {
            return Problem("Error while deleting order");
        }

        return Ok(isDeleted);
    }
}