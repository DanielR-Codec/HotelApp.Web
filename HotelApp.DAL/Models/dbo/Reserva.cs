using System;
using System.Collections.Generic;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public string? CodigoReserva { get; set; }
        public int IdHuesped { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public string? Estado { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
