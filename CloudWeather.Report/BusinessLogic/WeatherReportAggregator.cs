using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CloudWeather.Report.BusinessLogic
{
    /// <summary>
    /// Aggregates data from multiple external sources to build a weather report
    /// </summary>
    public interface IWeatherReportAggregator
    {
        /// <summary>
        /// Builds and return a weather report
        /// Persists weekly weather report data
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        
        //use database that WeatherReport service manages as a cache for a weatherreport that gets constructed so that if subsequent requests for the same weather report comes in prevent the service make http requests and avoid duplicate round trips for the same data
        public Task<WeatherReport> BuildWeeklyReport(string zip, int days);
    }

    
    public class WeatherReportAggregator: IWeatherReportAggregator
    {
        //use it to make http calls to other services
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        //holds configuration
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(IHttpClientFactory http, ILogger<WeatherReportAggregator> logger, IOptions<WeatherDataConfig> weatherDataConfig, WeatherReportDbContext db)
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildWeeklyReport(string zip, int days)
        {
            var httpClient = _http.CreateClient();
            
            //get and display precipitation data
            var precipData = await FetchPrecipitationData(httpClient, zip, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days: " +
                $"total snow {totalSnow}, rain: {totalRain}"
            );


            //get and display temperature data
            var temperatureData = await FetchTemperatureData(httpClient, zip, days);
            var averageHighTemp = temperatureData.Average(t=>t.TempHighF);
            var averageLowTemp = temperatureData.Average(t => t.TempLowF);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days: " +
                $"low temperature {averageLowTemp}, high temperature: {averageHighTemp}"
            );


            //build the Weather Report object
            var weatherReport = new WeatherReport
            {
                AverageHighF = Math.Round(averageHighTemp,1),
                AverageLowF = Math.Round(averageLowTemp,1),
                RainfallTotalinches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zip,
                CreatedOn = DateTime.UtcNow,

            };

            //Save the new object in the database
            // TODO: Use 'cached' reports instead of making roundtrips when data already fetched 
            _db.Add(weatherReport);
            await _db.SaveChangesAsync();

            return weatherReport;
        }

        private static decimal GetTotalRain(IEnumerable<PrecipitationModel> precipData) {
            var totalRain = precipData
                .Where(p => p.WeatherType == "rain")
                .Sum(p => p.AmountInches);
            return Math.Round(totalRain, 1);
        }
        private static decimal GetTotalSnow(IEnumerable<PrecipitationModel> precipData)
        {
            var totalSnow = precipData
                .Where(p => p.WeatherType == "snow")
                .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }


        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildTemperatureServiceEndpoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var temperatureData = await temperatureRecords
                .Content
                .ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptions);
            return temperatureData ?? new List<TemperatureModel>();
        }

        private async Task<List<PrecipitationModel>> FetchPrecipitationData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildPrecipitationServiceEndpoint(zip, days);
            var precipitationRecords = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptions = new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var precipitationData = await precipitationRecords
                .Content
                .ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializerOptions);
            return precipitationData ?? new List<PrecipitationModel>();
        }

        private string BuildTemperatureServiceEndpoint(string zip, int days)
        {
            var tempServiceProtocol = _weatherDataConfig.TemperatureDataProtocol;
            var tempServiceHost = _weatherDataConfig.TemperatureDataHost;
            var tempServicePort = _weatherDataConfig.TemperatureDataPort;
            //use this instead creating hard dependencies to coresponding service 
            return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
        }

        private string BuildPrecipitationServiceEndpoint(string zip, int days)
        {
            var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherDataConfig.PrecipDataHost;
            var precipServicePort = _weatherDataConfig.PrecipDataPort;
            //use this instead creating hard dependencies to coresponding service
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
        }


        
    }
    
}
