using System.Net;
using System.Text;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.PollyPolicies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;

    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }
    
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy
            .HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {   
                _logger.LogWarning("Fallback policy has been triggered: The request failed, returning dummy data");

                ProductDTO productDto = new ProductDTO(
                    ProductId: Guid.Empty, 
                    Category: "Temporarily Unavailable (Fallback)",
                    ProductName: "Temporarily Unavailable (Fallback)",
                    UnitPrice: 0,
                    QuantityInStock: 0
                );

                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                responseMessage.Content = new StringContent(JsonSerializer.Serialize(productDto), Encoding.UTF8, "application/json");
                
                return responseMessage;
            });

        return policy;
    }
}