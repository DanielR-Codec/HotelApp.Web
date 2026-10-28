using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Context
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Huesped> Huespedes { get; set; }
        public DbSet<TipoHabitacion> TiposHabitacion { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<DetalleReserva> DetallesReserva { get; set; }
    }
}
