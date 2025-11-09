namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(Guid ProductId, string? NewName);
