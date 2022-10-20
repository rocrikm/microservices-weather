namespace CloudWeather.Report.DataAccess
{
    public class WeatherReport
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal AverageHighF { get; set; }
        public decimal AverageLowF { get; set; }

        public decimal RainfallTotalinches { get; set; }
        public decimal SnowTotalinches { get; set; }
        public string ZipCode { get; set; }
    }
}
