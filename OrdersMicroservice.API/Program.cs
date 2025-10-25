using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClient;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;

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

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
});

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
});

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