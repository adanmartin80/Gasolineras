using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gasolineras.DTO
{
    public class PreciosCarburantes
    {
        public DateTime Fecha { get; set; }
        public List<EstacionesTerrestres> ListaEESSPrecio { get; set; }
    }
}
