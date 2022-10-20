using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var servicesConfig = config.GetSection("Services");

var tempServiceConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempServiceConfig["Host"];
var tempServicePort = tempServiceConfig["Port"];

var precipServiceConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipServiceConfig["Host"];
var precipServicePort = precipServiceConfig["Port"];


var zipCodes = new List<string> { 
"753178",
"124345",
"980980",
"435552",
"282090"
};

Console.WriteLine("Start Data Load");
var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");
var precipitationHttpClient = new HttpClient();
precipitationHttpClient.BaseAddress = new Uri($"http://{precipServiceHost}:{precipServicePort}");

//start feeding the DB with data
foreach (var zip in zipCodes) {
    Console.WriteLine($"Adding Data for Zip Code: {zip}");
    var from = DateTime.Now.AddYears(-2);
    var thru = DateTime.Now;

    for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1)) {
        var temps = PostTemp(zip, day, temperatureHttpClient);
        PostPrecip(temps[0], zip, day, precipitationHttpClient);
    }
}

void PostPrecip(int lowTemp, string zip, DateTime day, HttpClient precipitationHttpClient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1; //50% chance to have precipitation

    PrecipitationModel precipitation;

    if (isPrecip)
    {
        var precipInches = rand.Next(1, 16);
        if (lowTemp < 32)
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "snow",
                ZipCode = zip,
                CreatedOn = day

            };
        }
        else
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "rain",
                ZipCode = zip,
                CreatedOn = day

            };
        }
    }
    else {
        precipitation = new PrecipitationModel
        {
            AmountInches = 0,
            WeatherType = "fair",
            ZipCode = zip,
            CreatedOn = day

        };
    }
    
    // post the thing through the service into the database
    var precipResponse = precipitationHttpClient
    .PostAsJsonAsync("observation", precipitation)
    .Result;

    if (precipResponse.IsSuccessStatusCode) {
        Console.WriteLine($"Data point added for precipitation: Date: {day:d}"+
                          $"Zip: {zip} "+
                          $"Type: {precipitation.WeatherType} " +
                          $"Amount: {precipitation.AmountInches}"
            );
    }
}





// return value is a list of 2 values - lowTemp and highTemp
List<int> PostTemp(string zip, DateTime day, HttpClient temperatureHttpClient)
{
    var rand = new Random(); 
    var loTemp = rand.Next(0,100);
    var hiTemp = rand.Next(0,100);
    var hiLoTemps = new List<int> { loTemp, hiTemp};
    hiLoTemps.Sort();


    var temperatureObservation = new TemperatureModel
    {
        TempHighF = hiLoTemps[1],
        TempLowF = hiLoTemps[0],
        ZipCode = zip,
        CreatedOn = day
    };


    //send data points in DB using temperature service
    var tempresponse = temperatureHttpClient
        .PostAsJsonAsync("observation", temperatureObservation)
        .Result;

    if (tempresponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Data point added for temperature: Date: {day:d}" +
                          $"Zip: {zip} " +
                          $"Lo F: {hiLoTemps[0]} " +
                          $"Hi F: {hiLoTemps[1]} "
            );
    }
    else {
        Console.WriteLine(tempresponse.ToString());
    }

    return hiLoTemps;
}