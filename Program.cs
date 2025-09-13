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

// Register your services for dependency injection
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BlobService>(provider =>
{
    // Replace with your actual Azure Blob Storage connection string and container name
    var connectionString = "DefaultEndpointsProtocol=https;AccountName=filemanagerpocstore;AccountKey=UwVKHssAkyV1iUJaoY/3aSBP8hYgHx/ZXTSKR5As42cZmJsoer3eoPYdQhTq90i0cjPbfzxnsDk7+AStEltpXg==;EndpointSuffix=core.windows.net";
    var containerName = "filemanager-container";
    return new BlobService(connectionString, containerName);
});
builder.Services.AddSingleton<TableService>(provider =>
{
    // Replace with your actual Azure Table Storage connection string and table name
    var connectionString = "DefaultEndpointsProtocol=https;AccountName=filemanagerpocstore;AccountKey=UwVKHssAkyV1iUJaoY/3aSBP8hYgHx/ZXTSKR5As42cZmJsoer3eoPYdQhTq90i0cjPbfzxnsDk7+AStEltpXg==;EndpointSuffix=core.windows.net";
    var tableName = "FileMetaData";
    return new TableService(connectionString, tableName);
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
