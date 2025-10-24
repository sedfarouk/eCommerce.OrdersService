using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all orders asynchronously
    /// </summary>
    /// <returns>Returns all orders from the orders collection</returns>
    Task<IEnumerable<Order>> GetOrders();

    /// <summary>
    /// Retrieves all orders based on the specified condition asynchronously
    /// </summary>
    /// <param name="filter">The condition to filter orders</param>
    /// <returns>Returning a collection of matching orders</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    
    /// <summary>
    /// Retrieves a single order based on the specified condition asynchronously
    /// </summary>
    /// <param name="filter">The condition to filter orders</param>
    /// <returns>Returning matching order</returns>
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Add an order to the orders collection asynchronously
    /// </summary>
    /// <param name="order">The order to be added</param>
    /// <returns>Returns the added order object or null if unsuccessful</returns>
    Task<Order?> AddOrder(Order order);

    /// <summary>
    /// Updates an existing order asynchronously
    /// </summary>
    /// <param name="order">The order to be updated</param>
    /// <returns>Returns the updated order object or null if not found</returns>
    Task<Order?> UpdateOrder(Order order);
    
    /// <summary>
    /// Deletes an order asynchronously
    /// </summary>
    /// <param name="orderId">The Order id based on which we need to delete an order</param>
    /// <returns>Returns true if deletion is successful else false</returns>
    Task<bool> DeleteOrder(Guid orderId);
}