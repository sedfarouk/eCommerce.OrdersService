using System.Net;
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;

using System.Net.Http;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserId(Guid userId)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);

                // Fallback dummy data for non-success cases
                return new UserDTO(PersonName: "Temporarily Unavailable",
                    Email: "Temporarily unavailable",
                    Gender: "Temporarily Unavailable",
                    UserId: Guid.Empty);
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            return user ?? throw new ArgumentException("Invalid user id");
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is BrokenCircuitException)
        {
            if (ex is BrokenCircuitException)
            {
                _logger.LogError(ex, "Request failed because circuit breaker is in 'Open' state. Returning dummy data.");
            }
            
            // Network-level fallback
            Console.WriteLine($"[Warning] UsersMicroservice unreachable: {ex.Message}");
            return new UserDTO(PersonName: "Temporarily Unavailable",
                Email: "Temporarily unavailable",
                Gender: "Temporarily Unavailable",
                UserId: Guid.Empty);
        }
    }

}