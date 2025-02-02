using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gasolineras.DTO
{
    public class EstacionesTerrestresToZotac
    {
        public string CodigoPostal { get; set; }
        public string Direccion { get; set; }
        public string Horario { get; set; }
        public string Latitud { get; set; }
        public string Localidad { get; set; }
        public string Longitud { get; set; }
        public string UrlGoogleMaps
        {
            get
            {
                try
                {
                    var url = @$"https://www.google.com/maps/search/?api=1&query={Latitud.Replace(',', '.')},{Longitud.Replace(',', '.')}";
                    return url;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
        public string UrlWaze
        {
            get
            {
                try
                {
                    var url = @$"https://www.waze.com/ul?ll={Latitud.Replace(',', '.')}%2C{Longitud.Replace(',', '.')}&navigate=yes&zoom=17";
                    return url;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
        public string Margen { get; set; }
        public string Municipio { get; set; }
        public float PrecioGasoleoA { get; set; }
        public string Provincia { get; set; }
        public string Remision { get; set; }
        public string Rotulo { get; set; }
        public string TipoVenta { get; set; }
        public string BioEtanol { get; set; }
        public string stermetlico { get; set; }
    }
}
