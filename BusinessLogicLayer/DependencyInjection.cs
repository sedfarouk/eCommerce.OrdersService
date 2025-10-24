using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // TO DO: Add business logic layer services into the IoC container

        // After compilation, the source code would contain the assembly of all validators in the validators folder. This command auto registers all the validators so we don't manually register each (done with IValidator).
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        
        // Automapper - Similar to above, we use the assemblies so we don't add each mapping profile individually
        services.AddAutoMapper(typeof(OrderToOrderResponseMappingProfile).Assembly);

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}