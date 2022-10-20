using CloudWeather.temperature.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TemperatureDbContext>(
    opts => {
        opts.EnableSensitiveDataLogging();
        opts.EnableDetailedErrors();
        opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient
);

var app = builder.Build();


app.MapGet("/observation/{zip}", async (string zip, [FromQuery] int? days, TemperatureDbContext db) =>
{
    //this is actually the delegate method that will call into the precipitation service business logic

    //this is kind of a unit test that the service is responding
    //return Results.Ok(zip);

    //validate input
    if (days == null || days < 1 || days > 30)
    {
        return Results.BadRequest("Please provide a 'days' query parameter between 1 and 30");
    }

    var StartDate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.Temperature
        .Where(temp => temp.ZipCode == zip && temp.CreatedOn > StartDate)
        .ToListAsync();

    return Results.Ok(results);

});

// A mode to add datapoints to the database in a single DB transaction
app.MapPost("/observation", async (Temperature temp, TemperatureDbContext db) => {
    // is a good practice to separate the model that mimics the DB model, but a resource model dedicated for the UI
    temp.CreatedOn = temp.CreatedOn.ToUniversalTime();
    await db.AddAsync(temp);
    await db.SaveChangesAsync();
});

app.Run();

