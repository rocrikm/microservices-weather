using CloudWeather.Precipitation.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;


var builder = WebApplication.CreateBuilder(args);

// this MUST come BEFORE builder.Build()
builder.Services.AddDbContext<PrecipDbContext>(
    opts => {
        opts.EnableSensitiveDataLogging();
        opts.EnableDetailedErrors();
        opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient
);

var app = builder.Build();

//add the endpoint for calling the service
// first thing is the route
// than the parameters that we're going to pass => 
// the delagate method

// A method to interrogate the microservice
app.MapGet("/observation/{zip}", async (string zip, [FromQuery] int? days, PrecipDbContext db) =>
{
    //this is actually the delegate method that will call into the precipitation service business logic

    //this is kind of a unit test that the service is responding
    //return Results.Ok(zip);

    //validate input
    if (days == null || days < 1 || days > 30) {
        return Results.BadRequest("Please provide a 'days' query parameter between 1 and 30");
    }

    var StartDate = DateTime.UtcNow -TimeSpan.FromDays(days.Value);
    var results = await db.Precipitation
        .Where(precip => precip.ZipCode == zip && precip.CreatedOn > StartDate)
        .ToListAsync();

    return Results.Ok(results);

});

// A mode to add datapoints to the database in a single DB transaction
app.MapPost("/observation", async (Precipitation precip, PrecipDbContext db) => {
    // is a good practice to separate the model that mimics the DB model, but a resource model dedicated for the UI
    precip.CreatedOn = precip.CreatedOn.ToUniversalTime();
    await db.AddAsync(precip);
    await db.SaveChangesAsync();
});


app.Run();
