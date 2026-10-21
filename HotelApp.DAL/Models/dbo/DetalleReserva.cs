using System;
using System.Collections.Generic;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class DetalleReserva
    {
        public int IdDetalle { get; set; }
        public int IdReserva { get; set; }
        public int IdHabitacion { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
