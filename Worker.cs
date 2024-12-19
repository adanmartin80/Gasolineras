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
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                await CallApi(stoppingToken);

                await Delay(stoppingToken);

            }
        }

        private async Task Delay(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var next = _cron.GetNextOccurrence(DateTime.Now);
            var next2 = _cron2.GetNextOccurrence(DateTime.Now);

            if (next - now > next2 - now)
                await Task.Delay((next2 - now), stoppingToken);
            else
                await Task.Delay((next - now), stoppingToken);
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
                        .OrderBy(x => EstacionesTerrestres.GetPrecio(x.PrecioGasoleoA))
                        .Select(x => new EstacionesTerrestresToZotac
                            {
                                CodigoPostal = x.CP,
                                Direccion = x.Direccion,
                                Horario = x.Horario,
                                Latitud = x.Latitud,
                                Longitud = x.Longitud,
                                Localidad = x.Localidad,
                                Margen = x.Margen,
                                Municipio = x.Municipio,
                                PrecioGasoleoA = EstacionesTerrestres.GetPrecio(x.PrecioGasoleoA),
                                Provincia = x.Provincia,
                                Remision = x.Remision,
                                Rotulo = x.Rotulo,
                                TipoVenta = x.TipoVenta,
                                BioEtanol = x.BioEtanol,
                                stermetlico = x.stermetlico
                            });

                    //var result = comunidadMadrid
                    //    .Select(x => $"{x.Localidad}: Calle {x.Direccion}, url {x.UrlGoogleMaps}, Precio = {x.PrecioGasoleoA}");

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
