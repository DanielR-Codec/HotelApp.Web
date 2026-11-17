using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.Hotels;
using HotelApp.Web.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>Hotel management module.</summary>
    public class HotelsController : AppController
    {
        private readonly IHotelService _hotels;

        public HotelsController(IHotelService hotels)
        {
            _hotels = hotels;
        }

        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            var items = await LoadOrDefaultAsync(() => _hotels.GetAllAsync(includeInactive), Enumerable.Empty<Hotel>());
            return View(new HotelIndexViewModel { Items = items.ToList(), IncludeInactive = includeInactive });
        }

        [Authorize(Roles = UserRole.Administrator)]
        public IActionResult Create() => View("Form", new HotelFormViewModel());

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(HotelFormViewModel model)
        {
            model.HotelId = 0;
            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                await _hotels.AddAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Hotel '{model.Name}' registrado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return View("Form", model);
            }
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id)
        {
            var hotel = await FindAsync(id);
            if (hotel is null)
                return RedirectToAction(nameof(Index));

            return View("Form", HotelFormViewModel.FromEntity(hotel));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, HotelFormViewModel model)
        {
            model.HotelId = id;
            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                await _hotels.UpdateAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Hotel '{model.Name}' actualizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return View("Form", model);
            }
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await FindAsync(id);
            if (hotel is null)
                return RedirectToAction(nameof(Index));

            return View("ConfirmDelete", new ConfirmDeleteViewModel
            {
                Id = hotel.HotelId,
                EntityName = "el hotel",
                Warning = "El hotel quedará inactivo (no se borra de la base de datos). Solo se puede desactivar si no tiene bloques activos.",
                Fields =
                {
                    new("Nombre", hotel.Name),
                    new("Dirección", $"{hotel.Address}, {hotel.City}"),
                    new("Teléfono", hotel.Phone)
                }
            });
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _hotels.DeleteAsync(id, CurrentUserId);
                ShowSuccess("Hotel desactivado correctamente.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Hotel?> FindAsync(int id)
        {
            try
            {
                var hotel = await _hotels.GetByIdAsync(id);
                if (hotel is null)
                    ShowError("El hotel no existe.");
                return hotel;
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }
    }
}
