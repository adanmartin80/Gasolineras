using Gasolineras.Services;
using MQTTnet;
using MQTTnet.Client;

namespace Gasolineras
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var user = Environment.GetEnvironmentVariable("MQTT_USER", EnvironmentVariableTarget.User);
            var pass = Environment.GetEnvironmentVariable("MQTT_PASS", EnvironmentVariableTarget.User);
            var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY", EnvironmentVariableTarget.User);
            var accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY", EnvironmentVariableTarget.User);


            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton((provider) => new MqttClientOptionsBuilder()
                                                            .WithCredentials(user, pass)
                                                            .WithKeepAlivePeriod(TimeSpan.MaxValue)
                                                            .WithTcpServer("192.168.15.164", 1883)
            );
            builder.Services.AddTransient<MqttFactory>();
            builder.Services.AddTransient<ZotacMqttService>();

            var host = builder.Build();
            host.Run();
        }
    }
}