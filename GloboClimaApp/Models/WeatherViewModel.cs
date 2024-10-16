namespace GloboClimaApp.Models
{
    public class WeatherViewModel
    {
        public string CityName { get; set; }
        public WeatherInfo WeatherInfo { get; set; } 
        public List<string> FavoriteCities { get; set; }
    }

    public class WeatherInfo
    {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public Sys Sys { get; set; }
        public int Visibility { get; set; }
        public string Name { get; set; } 
    }

    public class Coord
    {
        public float Lon { get; set; }
        public float Lat { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class Main
    {
        public float Temp { get; set; }
        public float Feels_Like { get; set; }
        public float Temp_Min { get; set; }
        public float Temp_Max { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
    }

    public class Wind
    {
        public float Speed { get; set; }
    }

    public class Clouds
    {
        public int All { get; set; }
    }

    public class Sys
    {
        public long Sunrise { get; set; }
        public long Sunset { get; set; }
    }
}
