using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudWeather.DataLoader.Models
{
    //we don't want to share models btw services, but we want to have a contract to sync these
    public class TemperatureModel
    {
        public DateTime CreatedOn { get; set; }
        public decimal TempHighF { get; set; }
        public decimal TempLowF { get; set; }
        public string ZipCode { get; set; }
    }
}