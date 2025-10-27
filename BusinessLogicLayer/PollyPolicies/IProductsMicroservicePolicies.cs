using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.PollyPolicies;

public interface IProductsMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
}