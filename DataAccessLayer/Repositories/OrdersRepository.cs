using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private IMongoCollection<Order> _orders;
    private readonly string _collectionName = "orders";
    
    public OrdersRepository(IMongoDatabase mongoDb)
    {
        _orders = mongoDb.GetCollection<Order>(_collectionName);
    }
    
    public async Task<IEnumerable<Order>> GetOrders()
    {
        return (await _orders.FindAsync(FilterDefinition<Order>.Empty)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).ToList();
    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return await (await _orders.FindAsync(filter)).FirstOrDefaultAsync();
    }

    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderId = Guid.NewGuid();
        order._id = order.OrderId;

        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem._id = Guid.NewGuid();
        }

        await _orders.InsertOneAsync(order);
        return order;
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(item => item._id, order._id);
        
         Order? existingOrder = await (await _orders.FindAsync(filter)).FirstOrDefaultAsync();

         if (existingOrder == null)
         {
             return null;
         }

         ReplaceOneResult replaceOneResult = await _orders.ReplaceOneAsync(filter, order);

         if (replaceOneResult.ModifiedCount > 0)
         {
             return order;
         }

         return null;
    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, orderId);

        Order? existingOrder = await (await _orders.FindAsync(filter)).FirstOrDefaultAsync();
        
        if (existingOrder == null)
        {
            return false;
        }

        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
        
        return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
    }
}