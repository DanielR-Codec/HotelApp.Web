using System.Diagnostics;
using HotelApp.BL.Interfaces;
using HotelApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    public class HomeController : AppController
    {
        private readonly IHotelService _hotels;
        private readonly IBlockService _blocks;
        private readonly IFloorService _floors;
        private readonly IRoomService _rooms;
        private readonly IRoomTypeService _roomTypes;

        public HomeController(IHotelService hotels, IBlockService blocks, IFloorService floors, IRoomService rooms, IRoomTypeService roomTypes)
        {
            _hotels = hotels;
            _blocks = blocks;
            _floors = floors;
            _rooms = rooms;
            _roomTypes = roomTypes;
        }

        /// <summary>Dashboard with the modules and how many active records each one has.</summary>
        public async Task<IActionResult> Index()
        {
            var model = await LoadOrDefaultAsync(async () => new DashboardViewModel
            {
                Hotels = (await _hotels.GetAllAsync()).Count(),
                Blocks = (await _blocks.GetAllAsync()).Count(),
                Floors = (await _floors.GetAllAsync()).Count(),
                Rooms = (await _rooms.GetAllAsync()).Count(),
                RoomTypes = (await _roomTypes.GetAllAsync()).Count()
            }, new DashboardViewModel());

            return View(model);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
