using System;
using System.Collections.Generic;
using System.Text;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Repositories.dbo
{
    public class HuespedRepository : BaseRepository<Huesped>, IHuespedRepository
    {
        public HuespedRepository(HotelContext context) : base(context) { }
    }
}
