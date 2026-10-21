using System;
using System.Collections.Generic;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class TipoHabitacion
    {
        public int IdTipo { get; set; }
        public string? Nombre { get; set; }
        public int CapacidadMaxima { get; set; }
        public decimal TarifaBase { get; set; }
    }
}
