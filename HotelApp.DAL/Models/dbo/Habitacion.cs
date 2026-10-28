using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class Habitacion
    {
        [Key]
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public int Piso { get; set; }
        public int IdTipo { get; set; }
        public string EstadoActual { get; set; }
    }
}
