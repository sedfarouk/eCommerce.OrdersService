using System.Net;
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
using System.Net.Http;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;

    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/products/search/productid/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }

                throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
            }

            ProductDTO? productDto = await response.Content.ReadFromJsonAsync<ProductDTO>();

            return productDto;
        }

        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation blocks teh request since the request queue is full");

            return new ProductDTO(
                ProductId: Guid.NewGuid(),
                ProductName: "Temporarily Unavailable (Bulkhead)",
                UnitPrice: 0,
                QuantityInStock: 0,
                Category: "Temporarily Unavailable (Bulkhead)"
            );
        }
    }
}