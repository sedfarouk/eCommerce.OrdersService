using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.PollyPolicies;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add DAL & BLL Services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

// Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cors
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();

builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress =
        new Uri(
            $"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
})
.AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<UsersMicroservicePolicies>().GetCombinedPolicies());
    // .AddPolicyHandler(
    // builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy() 
    // )
    // .AddPolicyHandler(
    //     builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy()
    //     )
    //     .AddPolicyHandler(
    //         builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetTimeoutPolicy()
    //     );

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
}).AddPolicyHandler(
        builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallbackPolicy())
.AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<ProductsMicroservicePolicies>().GetBulkHeadIsolationPolicy());

var app = builder.Build();
     
// Exception Handling middleware
app.UseExceptionHandlingMiddleware();

app.UseRouting();

// Cors
app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();