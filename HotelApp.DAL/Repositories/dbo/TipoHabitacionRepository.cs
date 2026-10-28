using System;
using System.Collections.Generic;
using System.Text;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Repositories.dbo
{
    public class TipoHabitacionRepository : BaseRepository<TipoHabitacion>, ITipoHabitacionRepository
    {
        public TipoHabitacionRepository(HotelContext context) : base(context) { }
    }
}
