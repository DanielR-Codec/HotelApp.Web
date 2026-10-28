using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; }
        public int IdHuesped { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public string Estado { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
