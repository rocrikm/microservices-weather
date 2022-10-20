using CloudWeather.Report.BusinessLogic;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient();
builder.Services.AddTransient<IWeatherReportAggregator, WeatherReportAggregator>();
builder.Services.AddOptions();
builder.Services.Configure<WeatherDataConfig>(builder.Configuration.GetSection("WeatherDataConfig"));

builder.Services.AddDbContext<WeatherReportDbContext>(
    opts => {
        opts.EnableSensitiveDataLogging();
        opts.EnableDetailedErrors();
        opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient
);

var app = builder.Build();

// this is the most complex service because it read information from the user, it calls into 2 other services and aggregates the input for a user response when the info from the other services are recieved

app.MapGet(
    "/weather-report/{zip}", 
    async( string zip, [FromQuery] int? days, IWeatherReportAggregator weatherAgg) => {
        if (days == null || days < 1 || days > 30)
        {
            return Results.BadRequest("Please provide a 'days' query parameter between 1 and 30");
        }
        var report = await weatherAgg.BuildWeeklyReport(zip, days.Value);
        return Results.Ok(report);
});


app.Run();

