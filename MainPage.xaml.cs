using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq; // Make sure Newtonsoft.Json is installed via NuGet

 
namespace managmentapp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnWeatherButtonClicked(object sender, EventArgs e)
        {
            WeatherLabel.Text = "Fetching weather data..."; // Set a placeholder text to indicate loading
            string weatherData = await GetWeatherData();
            WeatherLabel.Text = weatherData; // Update the label with the weather data
        }

        private async Task<string> GetWeatherData()
        {
            string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current_weather=true&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,pressure_msl";

            using HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);

                // Log the JSON response for debugging
                Console.WriteLine(content);

                // Check if current_weather exists
                if (json["current_weather"] == null)
                {
                    return "Error: Current weather data is missing.";
                }

                // Parse the current weather data
                double temperature = (double)(json["current_weather"]["temperature"] ?? 0);
                double windSpeed = (double)(json["current_weather"]["windspeed"] ?? 0);
                double pressure = (double)(json["current_weather"]["pressure"] ?? 0); // Pressure data
                double humidity = (double)(json["current_weather"]["humidity"] ?? 0); // Humidity data

                // Build the current weather string
                string weatherData = $"Current Weather:\n" +
                                     $"Temperature: {temperature}°C\n" +
                                     $"Wind Speed: {windSpeed} m/s\n" +
                                     $"Pressure: {pressure} hPa\n" +
                                     $"Humidity: {humidity}%";

                // Checking if hourly data exists in the response
                if (json["hourly"] == null)
                {
                    return weatherData + "\nHourly data is not available.";
                }

                // Extract hourly data (temperature, humidity, and wind speed)
                var hourlyTemps = json["hourly"]?["temperature_2m"];
                var hourlyHumidity = json["hourly"]?["relative_humidity_2m"];
                var hourlyWindSpeed = json["hourly"]?["wind_speed_10m"];

                // If hourly data is incomplete, notify the user
                if (hourlyTemps == null || hourlyHumidity == null || hourlyWindSpeed == null)
                {
                    return weatherData + "\nHourly weather data is incomplete.";
                }

                // Format the hourly data
                string hourlyData = "\nHourly Forecast (Next 10 Hours):\n";
                int hoursToShow = Math.Min(10, hourlyTemps.Count()); // Ensure we don't exceed available data
                for (int i = 0; i < hoursToShow; i++)
                {
                    hourlyData += $"Hour {i}: Temp: {hourlyTemps[i]}°C, Humidity: {hourlyHumidity[i]}%, Wind Speed: {hourlyWindSpeed[i]} m/s\n";
                }

                // Return the full weather data (current + hourly)
                return weatherData + hourlyData;
            }
            catch (Exception ex)
            {
                // If an error occurs, return the error message
                return $"Unable to retrieve weather data: {ex.Message}";
            }
        }
    }
}



