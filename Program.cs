using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using AzureFileManager.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read connection strings from configuration (environment variables or appsettings.json)
string blobConnectionString = builder.Configuration["BLOB_CONN_STRING"] 
    ?? throw new Exception("BLOB_CONN_STRING is missing! Check your environment variables or appsettings.json.");
string tableConnectionString = builder.Configuration["TABLE_CONN_STRING"] 
    ?? throw new Exception("TABLE_CONN_STRING is missing! Check your environment variables or appsettings.json.");
string keyvaultName = builder.Configuration["KEYVAULT_NAME"] 
    ?? throw new Exception("KEYVAULT_NAME is missing!");
string tenantId = builder.Configuration["AZURE_TENANT_ID"] 
    ?? throw new Exception("AZURE_TENANT_ID is missing!");
string clientId = builder.Configuration["AZURE_CLIENT_ID"] 
    ?? throw new Exception("AZURE_CLIENT_ID is missing!");
string appInsightsKey = builder.Configuration["APPINSIGHTS_INSTRUMENTATION_KEY"] 
    ?? throw new Exception("APPINSIGHTS_INSTRUMENTATION_KEY is missing!");

// Blob and table names
var blobContainerName = "filemanager-container";  
var tableName = "FileMetadata";

// Register your services for dependency injection
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BlobService>(provider =>
    new BlobService(blobConnectionString, blobContainerName));
builder.Services.AddSingleton<TableService>(provider =>
    new TableService(tableConnectionString, tableName));

var app = builder.Build();

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Sample WeatherForecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
