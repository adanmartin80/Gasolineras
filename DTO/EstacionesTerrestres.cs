using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gasolineras.DTO
{
    public class EstacionesTerrestres
    {
        [JsonProperty("C.P.")]
        public string CP { get; set; }
        [JsonProperty("Dirección")]
        public string Direccion { get; set; }
        public string Horario { get; set; }
        public string Latitud { get; set; }
        public string Localidad { get; set; }

        [JsonProperty("Longitud (WGS84)")]
        public string Longitud { get; set; }
        [JsonIgnore]
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
        public string Margen { get; set; }
        public string Municipio { get; set; }

        [JsonProperty("Precio Biodiesel")]
        public string PrecioBiodiesel { get; set; }

        [JsonProperty("Precio Bioetanol")]
        public string PrecioBioetanol { get; set; }

        [JsonProperty("Precio Gas Natural Comprimido")]
        public string PrecioGasNaturalComprimido { get; set; }

        [JsonProperty("Precio Gas Natural Licuado")]
        public string PrecioGasNaturalLicuado { get; set; }

        [JsonProperty("Precio Gases licuados del petróleo")]
        public string PrecioGaseslicuadosdelpetrleo { get; set; }

        [JsonProperty("Precio Gasoleo A")]
        public string PrecioGasoleoA { get; set; }

        [JsonProperty("Precio Gasoleo B")]
        public string PrecioGasoleoB { get; set; }

        [JsonProperty("Precio Gasoleo Premium")]
        public string PrecioGasoleoPremium { get; set; }

        [JsonProperty("Precio Gasolina 95 E10")]
        public string PrecioGasolina95E10 { get; set; }

        [JsonProperty("Precio Gasolina 95 E5")]
        public string PrecioGasolina95E5 { get; set; }

        [JsonProperty("Precio Gasolina 95 E5 Premium")]
        public string PrecioGasolina95E5Premium { get; set; }

        [JsonProperty("Precio Gasolina 98 E10")]
        public string PrecioGasolina98E10 { get; set; }

        [JsonProperty("Precio Gasolina 98 E5")]
        public string PrecioGasolina98E5 { get; set; }

        [JsonProperty("Precio Hidrogeno")]
        public string PrecioHidrogeno { get; set; }
        public string Provincia { get; set; }
        [JsonProperty("Remisión")]
        public string Remision { get; set; }
        [JsonProperty("Rótulo")]
        public string Rotulo { get; set; }

        [JsonProperty("Tipo Venta")]
        public string TipoVenta { get; set; }

        [JsonProperty("% BioEtanol")]
        public string BioEtanol { get; set; }

        [JsonProperty("% Éster metílico")]
        public string stermetlico { get; set; }
        public string IDEESS { get; set; }
        public string IDMunicipio { get; set; }
        public string IDProvincia { get; set; }
        public string IDCCAA { get; set; }

        public static float GetPrecio(string precio)
        {
            _ = float.TryParse(precio, out float castPrecio);

            return castPrecio;
        }
    }
}
