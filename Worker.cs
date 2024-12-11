using Gasolineras.DTO;
using Newtonsoft.Json;
using NCrontab;

namespace Gasolineras
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string schedule = "* 23 * * *"; // every hour
        private readonly CrontabSchedule _cron = CrontabSchedule.Parse(schedule);

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                CallApi(stoppingToken);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var utcNow = DateTime.UtcNow;
                var nextUtc = _cron.GetNextOccurrence(utcNow);
                await Task.Delay(nextUtc - utcNow, stoppingToken);
            }
        }

        private async Task CallApi(CancellationToken stoppingToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"https://sedeaplicaciones.minetur.gob.es/ServiciosRESTCarburantes/PreciosCarburantes/EstacionesTerrestres");

                try
                {
                    var response = await client.GetAsync(string.Empty, stoppingToken);
                    var message = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(message)) return;

                    var precios = JsonConvert.DeserializeObject<PreciosCarburantes>(message);
                    if (precios == null) return;

                    var comunidadMadrid = precios.ListaEESSPrecio
                        .Where(x => x.Localidad.Contains("Madrid", StringComparison.CurrentCultureIgnoreCase))
                        .Where(x => !string.IsNullOrEmpty(x.PrecioGasoleoA))
                        .OrderBy(x => EstacionesTerrestres.GetPrecio(x.PrecioGasoleoA));

                    var result = comunidadMadrid
                        .Select(x => $"{x.Localidad}: Calle {x.Direccion}, Precio = {EstacionesTerrestres.GetPrecio(x.PrecioGasoleoA)}");

                    Console.WriteLine(string.Join("\n\r", result));

                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError("Worker running at: {time}", DateTimeOffset.Now);
                        _logger.LogError("Error: {error}", ex);
                    }
                }
                
            }
        }
    }
}
