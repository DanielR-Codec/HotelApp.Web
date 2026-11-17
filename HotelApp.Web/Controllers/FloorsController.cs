using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.Floors;
using HotelApp.Web.Models.Shared;
using HotelApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>Floor management module.</summary>
    public class FloorsController : AppController
    {
        private readonly IFloorService _floors;
        private readonly StructureLookupService _lookupService;

        public FloorsController(IFloorService floors, StructureLookupService lookupService)
        {
            _floors = floors;
            _lookupService = lookupService;
        }

        public async Task<IActionResult> Index(int? hotelId = null, int? blockId = null, bool includeInactive = false)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            var floors = await LoadOrDefaultAsync(() => _floors.GetAllAsync(hotelId, blockId, includeInactive), Enumerable.Empty<Floor>());

            return View(new FloorIndexViewModel
            {
                HotelId = hotelId,
                BlockId = blockId,
                IncludeInactive = includeInactive,
                HotelOptions = lookup.HotelOptions(hotelId),
                BlockOptions = lookup.BlockOptions(blockId, hotelId),
                Items = floors
                    .Select(f => new FloorListItemViewModel
                    {
                        FloorId = f.FloorId,
                        HotelName = lookup.HotelName(lookup.HotelIdOfBlock(f.BlockId) ?? 0),
                        BlockName = lookup.BlockName(f.BlockId),
                        Number = f.Number,
                        Description = f.Description,
                        IsActive = f.IsActive
                    })
                    .OrderBy(f => f.HotelName).ThenBy(f => f.BlockName).ThenBy(f => f.Number)
                    .ToList()
            });
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(int? blockId = null)
        {
            return await FormAsync(new FloorFormViewModel { BlockId = blockId ?? 0 });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(FloorFormViewModel model)
        {
            model.FloorId = 0;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _floors.AddAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Piso {model.Number} registrado correctamente.");
                return RedirectToAction(nameof(Index), new { blockId = model.BlockId });
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
            var floor = await FindAsync(id);
            if (floor is null)
                return RedirectToAction(nameof(Index));

            return await FormAsync(FloorFormViewModel.FromEntity(floor));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, FloorFormViewModel model)
        {
            model.FloorId = id;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _floors.UpdateAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Piso {model.Number} actualizado correctamente.");
                return RedirectToAction(nameof(Index), new { blockId = model.BlockId });
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
            var floor = await FindAsync(id);
            if (floor is null)
                return RedirectToAction(nameof(Index));

            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);

            return View("ConfirmDelete", new ConfirmDeleteViewModel
            {
                Id = floor.FloorId,
                EntityName = "el piso",
                Warning = "El piso quedará inactivo (no se borra de la base de datos). Solo se puede desactivar si no tiene habitaciones activas.",
                Fields =
                {
                    new("Hotel / Bloque", lookup.BlockLabel(floor.BlockId)),
                    new("Piso", floor.Number.ToString()),
                    new("Descripción", floor.Description ?? "—")
                }
            });
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _floors.DeleteAsync(id, CurrentUserId);
                ShowSuccess("Piso desactivado correctamente.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> FormAsync(FloorFormViewModel model)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            model.BlockOptions = lookup.BlockOptions(model.BlockId);
            return View("Form", model);
        }

        private async Task<Floor?> FindAsync(int id)
        {
            try
            {
                var floor = await _floors.GetByIdAsync(id);
                if (floor is null)
                    ShowError("El piso no existe.");
                return floor;
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }
    }
}
