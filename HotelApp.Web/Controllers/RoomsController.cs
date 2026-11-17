using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.Rooms;
using HotelApp.Web.Models.Shared;
using HotelApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelApp.Web.Controllers
{
    /// <summary>Room management module (CU-011 / RF-001) and manual status change (CU-012 / RF-010).</summary>
    public class RoomsController : AppController
    {
        private static readonly string[] ManualStatuses = { RoomStatus.Available, RoomStatus.Cleaning, RoomStatus.Maintenance };

        private readonly IRoomService _rooms;
        private readonly IRoomTypeService _roomTypes;
        private readonly StructureLookupService _lookupService;

        public RoomsController(IRoomService rooms, IRoomTypeService roomTypes, StructureLookupService lookupService)
        {
            _rooms = rooms;
            _roomTypes = roomTypes;
            _lookupService = lookupService;
        }

        public async Task<IActionResult> Index(int? hotelId = null, int? blockId = null, int? floorId = null, bool includeInactive = false)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            var roomTypes = (await LoadOrDefaultAsync(() => _roomTypes.GetAllAsync(), Enumerable.Empty<RoomType>()))
                .ToDictionary(t => t.RoomTypeId);
            var rooms = await LoadOrDefaultAsync(() => _rooms.GetAllAsync(hotelId, blockId, floorId, includeInactive), Enumerable.Empty<Room>());

            return View(new RoomIndexViewModel
            {
                HotelId = hotelId,
                BlockId = blockId,
                FloorId = floorId,
                IncludeInactive = includeInactive,
                HotelOptions = lookup.HotelOptions(hotelId),
                BlockOptions = lookup.BlockOptions(blockId, hotelId),
                FloorOptions = lookup.FloorOptions(floorId, hotelId, blockId),
                ManualStatuses = ManualStatuses,
                Items = rooms.Select(r =>
                {
                    lookup.Floors.TryGetValue(r.FloorId, out var floor);
                    roomTypes.TryGetValue(r.RoomTypeId, out var roomType);

                    return new RoomListItemViewModel
                    {
                        RoomId = r.RoomId,
                        Number = r.Number,
                        HotelName = lookup.HotelName(lookup.HotelIdOfFloor(r.FloorId) ?? 0),
                        BlockName = floor is null ? "—" : lookup.BlockName(floor.BlockId),
                        FloorNumber = floor?.Number ?? 0,
                        RoomTypeName = roomType?.Name ?? "—",
                        MaxCapacity = roomType?.MaxCapacity ?? 0,
                        BaseRate = roomType?.BaseRate ?? 0,
                        CurrentStatus = r.CurrentStatus,
                        IsActive = r.IsActive
                    };
                })
                .OrderBy(r => r.HotelName).ThenBy(r => r.BlockName).ThenBy(r => r.FloorNumber).ThenBy(r => r.Number)
                .ToList()
            });
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(int? floorId = null)
        {
            return await FormAsync(new RoomFormViewModel { FloorId = floorId ?? 0 });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(RoomFormViewModel model)
        {
            model.RoomId = 0;
            model.IsActive = true;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _rooms.AddAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Habitación {model.Number} registrada correctamente.");
                return RedirectToAction(nameof(Index), new { floorId = model.FloorId });
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return await FormAsync(model);
            }
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await FindAsync(id);
            if (room is null)
                return RedirectToAction(nameof(Index));

            return await FormAsync(RoomFormViewModel.FromEntity(room));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, RoomFormViewModel model)
        {
            model.RoomId = id;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _rooms.UpdateAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Habitación {model.Number} actualizada correctamente.");
                return RedirectToAction(nameof(Index), new { floorId = model.FloorId });
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return await FormAsync(model);
            }
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await FindAsync(id);
            if (room is null)
                return RedirectToAction(nameof(Index));

            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            var roomType = await LoadOrDefaultAsync(() => _roomTypes.GetByIdAsync(room.RoomTypeId), null);

            return View("ConfirmDelete", new ConfirmDeleteViewModel
            {
                Id = room.RoomId,
                EntityName = "la habitación",
                Warning = "La habitación quedará dada de baja (RF-001: borrado lógico). No se permite si tiene reservas vigentes.",
                Fields =
                {
                    new("Número", room.Number),
                    new("Ubicación", lookup.FloorLabel(room.FloorId)),
                    new("Tipo", roomType?.Name ?? "—"),
                    new("Estado", room.CurrentStatus)
                }
            });
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _rooms.DeleteAsync(id, CurrentUserId);
                ShowSuccess("Habitación dada de baja correctamente.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>CU-012 / RF-010: any logged-in user (receptionist or administrator) can change the status by hand.</summary>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus, int? hotelId, int? blockId, int? floorId, bool includeInactive = false)
        {
            try
            {
                await _rooms.ChangeStatusAsync(id, newStatus, CurrentUserId);
                ShowSuccess($"Estado cambiado a '{newStatus}'.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index), new { hotelId, blockId, floorId, includeInactive });
        }

        private async Task<IActionResult> FormAsync(RoomFormViewModel model)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            var roomTypes = await LoadOrDefaultAsync(() => _roomTypes.GetAllAsync(), Enumerable.Empty<RoomType>());

            model.FloorOptions = lookup.FloorOptions(model.FloorId);
            model.RoomTypeOptions = roomTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem($"{t.Name} ({t.MaxCapacity} pers. — RD$ {t.BaseRate:N2})", t.RoomTypeId.ToString(), t.RoomTypeId == model.RoomTypeId))
                .ToList();

            return View("Form", model);
        }

        private async Task<Room?> FindAsync(int id)
        {
            try
            {
                var room = await _rooms.GetByIdAsync(id);
                if (room is null)
                    ShowError("La habitación no existe.");
                return room;
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }
    }
}
