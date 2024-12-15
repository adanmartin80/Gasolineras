using Gasolineras.DTO;
using Newtonsoft.Json;
using NCrontab;
using Gasolineras.Services;

namespace Gasolineras
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ZotacMqttService _mqttService;
        private const string schedule = "* 15 * * *"; // Esperamos hasta las 3 de la tarde
        private const string schedule2 = "* 23 * * *"; // Esperamos hasta las 11 de la noche
        private readonly CrontabSchedule _cron = CrontabSchedule.Parse(schedule);
        private readonly CrontabSchedule _cron2 = CrontabSchedule.Parse(schedule2);


        public Worker(ILogger<Worker> logger, ZotacMqttService mqttService)
        {
            _logger = logger;
            _mqttService = mqttService;
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
                var nextUtc2 = _cron2.GetNextOccurrence(utcNow);

                if (nextUtc.Subtract(utcNow) > nextUtc2.Subtract(utcNow))
                    await Task.Delay(nextUtc2 - utcNow, stoppingToken);
                else
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

                    //var result = comunidadMadrid
                    //    .Select(x => $"{x.Localidad}: Calle {x.Direccion}, url {x.UrlGoogleMaps}, Precio = {EstacionesTerrestres.GetPrecio(x.PrecioGasoleoA)}");

                    var cheapestPrice = comunidadMadrid.FirstOrDefault();
                    await _mqttService.Publish(PublishTopics.Zotac.CheapestPrice, cheapestPrice);
                    await _mqttService.Publish(PublishTopics.Zotac.All, comunidadMadrid.ToList());

                    //Console.WriteLine(string.Join("\n\r", result));

                    //Console.WriteLine(message);
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
