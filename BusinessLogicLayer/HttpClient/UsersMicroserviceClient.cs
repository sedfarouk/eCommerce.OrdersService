using System.Net;
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

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
            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);

                // Fallback dummy data for non-success cases
                return new UserDTO(PersonName: "Temporarily Unavailable (circuit breaker)",
                    Email: "Temporarily unavailable (circuit breaker)",
                    Gender: "Temporarily Unavailable (circuit breaker)",
                    UserId: Guid.Empty);
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            return user ?? throw new ArgumentException("Invalid user id");
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is BrokenCircuitException)
        {
            if (ex is BrokenCircuitException)
            {
                _logger.LogError(ex,
                    "Request failed because circuit breaker is in 'Open' state. Returning dummy data.");
            }

            // Network-level fallback
            Console.WriteLine($"[Warning] UsersMicroservice unreachable: {ex.Message}");
            return new UserDTO(PersonName: "Temporarily Unavailable (circuit breaker)",
                Email: "Temporarily unavailable (circuit breaker)",
                Gender: "Temporarily Unavailable (circuit breaker)",
                UserId: Guid.Empty);
        }

        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occured while fetching user data. Returning dummy data");

            return new UserDTO(
                PersonName: "Temporarily Unavailable (timeout)",
                Email: "Temporarily unavailable (timeout)",
                Gender: "Temporarily Unavailable (timeout)",
                UserId: Guid.Empty
            );
        }
    }

}