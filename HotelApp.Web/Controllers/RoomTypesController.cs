using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.RoomTypes;
using HotelApp.Web.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>Room type maintenance (CU-010 / RF-002). Rates can only be changed by the Administrator (RN-014).</summary>
    public class RoomTypesController : AppController
    {
        private readonly IRoomTypeService _roomTypes;

        public RoomTypesController(IRoomTypeService roomTypes)
        {
            _roomTypes = roomTypes;
        }

        public async Task<IActionResult> Index()
        {
            var items = await LoadOrDefaultAsync(() => _roomTypes.GetAllAsync(), Enumerable.Empty<RoomType>());
            return View(items.OrderBy(t => t.Name).ToList());
        }

        [Authorize(Roles = UserRole.Administrator)]
        public IActionResult Create() => View("Form", new RoomTypeFormViewModel());

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(RoomTypeFormViewModel model)
        {
            model.RoomTypeId = 0;
            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                await _roomTypes.AddAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Tipo de habitación '{model.Name}' registrado correctamente.");
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
            var roomType = await FindAsync(id);
            if (roomType is null)
                return RedirectToAction(nameof(Index));

            return View("Form", RoomTypeFormViewModel.FromEntity(roomType));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, RoomTypeFormViewModel model)
        {
            model.RoomTypeId = id;
            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                await _roomTypes.UpdateAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Tipo de habitación '{model.Name}' actualizado correctamente.");
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
            var roomType = await FindAsync(id);
            if (roomType is null)
                return RedirectToAction(nameof(Index));

            return View("ConfirmDelete", new ConfirmDeleteViewModel
            {
                Id = roomType.RoomTypeId,
                EntityName = "el tipo de habitación",
                ActionVerb = "Eliminar",
                Warning = "El tipo se elimina definitivamente. No se permite si alguna habitación lo usa.",
                Fields =
                {
                    new("Nombre", roomType.Name),
                    new("Capacidad máxima", $"{roomType.MaxCapacity} persona(s)"),
                    new("Tarifa base", $"RD$ {roomType.BaseRate:N2}")
                }
            });
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _roomTypes.DeleteAsync(id, CurrentUserId);
                ShowSuccess("Tipo de habitación eliminado correctamente.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<RoomType?> FindAsync(int id)
        {
            try
            {
                var roomType = await _roomTypes.GetByIdAsync(id);
                if (roomType is null)
                    ShowError("El tipo de habitación no existe.");
                return roomType;
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }
    }
}
