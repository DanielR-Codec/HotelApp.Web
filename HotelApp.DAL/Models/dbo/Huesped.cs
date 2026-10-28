using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class Huesped
    {
        [Key]
        public int IdHuesped { get; set; }
        public string DocumentoIdentidad { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
    }
}
