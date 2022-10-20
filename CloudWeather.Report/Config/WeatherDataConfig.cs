namespace CloudWeather.Report.Config
{
    public class WeatherDataConfig
    {
        //need a way to configure collaboration btw services
        public string PrecipDataProtocol { get; set; } = string.Empty;
        public string PrecipDataHost { get; set; } = string.Empty;
        public string PrecipDataPort { get; set; } = string.Empty;
        public string TemperatureDataProtocol { get; set; } = string.Empty;
        public string TemperatureDataHost { get; set; } = string.Empty;
        public string TemperatureDataPort { get; set; } = string.Empty;

    }
}
