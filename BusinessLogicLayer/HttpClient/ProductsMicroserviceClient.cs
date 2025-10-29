using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
using System.Net.Http;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            string cacheKey = $"product:{productId}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedProduct is not null)
            {
                ProductDTO productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct)!;

                return productFromCache;
            }
            
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

            string cacheProduct = JsonSerializer.Serialize(productDto);

            DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();

            distributedCacheEntryOptions
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            string cacheKeyToWrite = $"product: {productId}";
            
            await _distributedCache.SetStringAsync(cacheKeyToWrite, cacheProduct, distributedCacheEntryOptions);

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