using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class TipoHabitacion
    {
        [Key]
        public int IdTipo { get; set; }
        public string Nombre { get; set; }
        public int CapacidadMaxima { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TarifaBase { get; set; }
    }
}
