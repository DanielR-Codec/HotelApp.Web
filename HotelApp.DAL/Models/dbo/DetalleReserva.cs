using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HotelApp.DAL.Models.dbo
{
    public class DetalleReserva
    {
        [Key]
        public int IdDetalle { get; set; }
        public int IdReserva { get; set; }
        public int IdHabitacion { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
