using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using AzureFileManager.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read connection strings from configuration (environment variables or appsettings.json)
var blobConnectionString = builder.Configuration["Azure:BlobStorageConnectionString"];
var tableConnectionString = builder.Configuration["Azure:TableStorageConnectionString"];
var blobContainerName = "BLOB_CONTAINER_NAME"; // safe to commit, not a secret
var tableName = "TABLE_NAME"; // safe to commit, not a secret

// Register your services for dependency injection
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BlobService>(provider =>
{
    return new BlobService(blobConnectionString, blobContainerName);
});
builder.Services.AddSingleton<TableService>(provider =>
{
    return new TableService(tableConnectionString, tableName);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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
