using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
using System.Net.Http;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO> GetProductByProductId(Guid productId)
    {
        ProductDTO? productDto = await _httpClient.GetFromJsonAsync<ProductDTO>($"api/products/search/{productId}");

        if (productDto == null)
        {
            return null;
        }

        return productDto;
    }
}