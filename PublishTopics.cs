using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gasolineras.Services
{
    public static class PublishTopics
    {
        public static class Zotac
        {
            /// <summary>
            /// Usado para publicar el topic en el MQTT del Zotac.
            /// </summary>
            public static string CheapestPrice { get; private set; } = "homeassistant/petrolstation/cheapestprice";
            public static string All { get; private set; } = "homeassistant/petrolstation/all";
            public static string HomeAssistantOnline { get; private set; } = "homeassistant/status";
        }
    }
}
