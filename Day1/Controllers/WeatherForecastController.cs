using Day1.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using Day1.Data;

namespace Day1.Controllers
{
    [ApiController]    
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class WeatherForecastController(IMemoryCache MemoryCache) : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "GetWeatherForecast")]
        [CheckPermission(Permission.Read)]
        public IEnumerable<WeatherForecast> Get()
        {
            Log.Information("WeatherForecastController.Get called at {Time}", DateTime.UtcNow);

            var cacheKey = "WeatherForecast";
            if (!MemoryCache.TryGetValue(cacheKey, out IEnumerable<WeatherForecast> weatherForecasts))
            {
                // Key not in cache, so generate data
                weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).ToArray();

                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                // Save data in cache
                MemoryCache.Set(cacheKey, weatherForecasts, cacheOptions);
            }
            else
            {
                Log.Information("Weather forecast retrieved from cache at {Time}", DateTime.UtcNow);
            }

            return weatherForecasts;
        }
    }
}
