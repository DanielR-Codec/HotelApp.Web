using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.Blocks;
using HotelApp.Web.Models.Shared;
using HotelApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>Block (building / tower / wing) management module.</summary>
    public class BlocksController : AppController
    {
        private readonly IBlockService _blocks;
        private readonly StructureLookupService _lookupService;

        public BlocksController(IBlockService blocks, StructureLookupService lookupService)
        {
            _blocks = blocks;
            _lookupService = lookupService;
        }

        public async Task<IActionResult> Index(int? hotelId = null, bool includeInactive = false)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            var blocks = await LoadOrDefaultAsync(() => _blocks.GetAllAsync(hotelId, includeInactive), Enumerable.Empty<Block>());

            return View(new BlockIndexViewModel
            {
                HotelId = hotelId,
                IncludeInactive = includeInactive,
                HotelOptions = lookup.HotelOptions(hotelId),
                Items = blocks
                    .Select(b => new BlockListItemViewModel
                    {
                        BlockId = b.BlockId,
                        HotelName = lookup.HotelName(b.HotelId),
                        Name = b.Name,
                        Description = b.Description,
                        IsActive = b.IsActive
                    })
                    .OrderBy(b => b.HotelName).ThenBy(b => b.Name)
                    .ToList()
            });
        }

        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(int? hotelId = null)
        {
            return await FormAsync(new BlockFormViewModel { HotelId = hotelId ?? 0 });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create(BlockFormViewModel model)
        {
            model.BlockId = 0;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _blocks.AddAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Bloque '{model.Name}' registrado correctamente.");
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
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
            var block = await FindAsync(id);
            if (block is null)
                return RedirectToAction(nameof(Index));

            return await FormAsync(BlockFormViewModel.FromEntity(block));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, BlockFormViewModel model)
        {
            model.BlockId = id;
            if (!ModelState.IsValid)
                return await FormAsync(model);

            try
            {
                await _blocks.UpdateAsync(model.ToEntity(), CurrentUserId);
                ShowSuccess($"Bloque '{model.Name}' actualizado correctamente.");
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
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
            var block = await FindAsync(id);
            if (block is null)
                return RedirectToAction(nameof(Index));

            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);

            return View("ConfirmDelete", new ConfirmDeleteViewModel
            {
                Id = block.BlockId,
                EntityName = "el bloque",
                Warning = "El bloque quedará inactivo (no se borra de la base de datos). Solo se puede desactivar si no tiene pisos activos.",
                Fields =
                {
                    new("Hotel", lookup.HotelName(block.HotelId)),
                    new("Bloque", block.Name),
                    new("Descripción", block.Description ?? "—")
                }
            });
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _blocks.DeleteAsync(id, CurrentUserId);
                ShowSuccess("Bloque desactivado correctamente.");
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> FormAsync(BlockFormViewModel model)
        {
            var lookup = await LoadOrDefaultAsync(() => _lookupService.LoadAsync(), StructureLookup.Empty);
            model.HotelOptions = lookup.HotelOptions(model.HotelId);
            return View("Form", model);
        }

        private async Task<Block?> FindAsync(int id)
        {
            try
            {
                var block = await _blocks.GetByIdAsync(id);
                if (block is null)
                    ShowError("El bloque no existe.");
                return block;
            }
            catch (HotelAppException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }
    }
}
