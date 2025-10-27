using System.Net;
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;

using System.Net.Http;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    public UsersMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
        catch (HttpRequestException ex)
        {
            // Network-level fallback
            Console.WriteLine($"[Warning] UsersMicroservice unreachable: {ex.Message}");
            return new UserDTO(PersonName: "Temporarily Unavailable",
                Email: "Temporarily unavailable",
                Gender: "Temporarily Unavailable",
                UserId: Guid.Empty);
        }
    }

}