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

                var comunidadMadrid = await CallApiServiciosRESTCarburantes("Madrid", stoppingToken);
                await _mqttService.Publish(PublishTopics.Zotac.CheapestPrice, comunidadMadrid.FirstOrDefault());
                await _mqttService.Publish(PublishTopics.Zotac.All, comunidadMadrid.ToList());

                await Delay(stoppingToken);

            }
        }

        private async Task Delay(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var next = _cron.GetNextOccurrence(DateTime.Now.AddMinutes(1));
            var next2 = _cron2.GetNextOccurrence(DateTime.Now.AddMinutes(1));

            if (next - now > next2 - now)
                await Task.Delay((next2 - now), stoppingToken);
            else
                await Task.Delay((next - now), stoppingToken);
        }

        private async Task<IEnumerable<EstacionesTerrestresToZotac>> CallApiServiciosRESTCarburantes(string comunidad, CancellationToken stoppingToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"https://sedeaplicaciones.minetur.gob.es/ServiciosRESTCarburantes/PreciosCarburantes/EstacionesTerrestres");

                try
                {
                    var response = await client.GetAsync(string.Empty, stoppingToken);
                    var message = await response.Content.ReadAsStringAsync(stoppingToken);

                    if (string.IsNullOrEmpty(message)) return [];

                    var precios = JsonConvert.DeserializeObject<PreciosCarburantes>(message);
                    if (precios == null) return [];

                    var comunidadMadrid = precios.ListaEESSPrecio
                        .Where(x => x.Localidad.Contains(comunidad, StringComparison.CurrentCultureIgnoreCase))
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

                    return comunidadMadrid;
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError("Worker running at: {time}", DateTimeOffset.Now);
                        _logger.LogError("Error: {error}", ex);
                    }

                    return [];
                }
                
            }
        }
    }
}
