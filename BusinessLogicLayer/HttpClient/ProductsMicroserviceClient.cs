using System.Net;
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

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
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
}