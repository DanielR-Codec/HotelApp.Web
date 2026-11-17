using HotelApp.BL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelApp.Web.Services
{
    /// <summary>
    /// Snapshot of the hotel structure (Hotel → Block → Floor) used by the views
    /// to show names instead of ids and to fill the drop-down lists.
    /// </summary>
    public class StructureLookup
    {
        public static readonly StructureLookup Empty = new(Array.Empty<Hotel>(), Array.Empty<Block>(), Array.Empty<Floor>());

        public IReadOnlyDictionary<int, Hotel> Hotels { get; }
        public IReadOnlyDictionary<int, Block> Blocks { get; }
        public IReadOnlyDictionary<int, Floor> Floors { get; }

        public StructureLookup(IEnumerable<Hotel> hotels, IEnumerable<Block> blocks, IEnumerable<Floor> floors)
        {
            Hotels = hotels.ToDictionary(h => h.HotelId);
            Blocks = blocks.ToDictionary(b => b.BlockId);
            Floors = floors.ToDictionary(f => f.FloorId);
        }

        public string HotelName(int hotelId) => Hotels.TryGetValue(hotelId, out var h) ? h.Name : "—";

        public string BlockName(int blockId) => Blocks.TryGetValue(blockId, out var b) ? b.Name : "—";

        public int? HotelIdOfBlock(int blockId) => Blocks.TryGetValue(blockId, out var b) ? b.HotelId : null;

        public int? HotelIdOfFloor(int floorId) => Floors.TryGetValue(floorId, out var f) ? HotelIdOfBlock(f.BlockId) : null;

        public string BlockLabel(int blockId)
        {
            return Blocks.TryGetValue(blockId, out var b) ? $"{HotelName(b.HotelId)} — {b.Name}" : "—";
        }

        public string FloorLabel(int floorId)
        {
            return Floors.TryGetValue(floorId, out var f) ? $"{BlockLabel(f.BlockId)} — Piso {f.Number}" : "—";
        }

        /// <summary>Active hotels, plus the selected one even if it is inactive (so editing never loses the current value).</summary>
        public IEnumerable<SelectListItem> HotelOptions(int? selectedId = null)
        {
            return Hotels.Values
                .Where(h => h.IsActive || h.HotelId == selectedId)
                .OrderBy(h => h.Name)
                .Select(h => new SelectListItem(h.Name, h.HotelId.ToString(), h.HotelId == selectedId))
                .ToList();
        }

        public IEnumerable<SelectListItem> BlockOptions(int? selectedId = null, int? hotelId = null)
        {
            return Blocks.Values
                .Where(b => (b.IsActive || b.BlockId == selectedId) && (hotelId == null || b.HotelId == hotelId))
                .Select(b => new { b.BlockId, Label = BlockLabel(b.BlockId) })
                .OrderBy(b => b.Label)
                .Select(b => new SelectListItem(b.Label, b.BlockId.ToString(), b.BlockId == selectedId))
                .ToList();
        }

        public IEnumerable<SelectListItem> FloorOptions(int? selectedId = null, int? hotelId = null, int? blockId = null)
        {
            return Floors.Values
                .Where(f => (f.IsActive || f.FloorId == selectedId)
                         && (blockId == null || f.BlockId == blockId)
                         && (hotelId == null || HotelIdOfBlock(f.BlockId) == hotelId))
                .Select(f => new { f.FloorId, f.Number, Label = BlockLabel(f.BlockId) })
                .OrderBy(f => f.Label).ThenBy(f => f.Number)
                .Select(f => new SelectListItem($"{f.Label} — Piso {f.Number}", f.FloorId.ToString(), f.FloorId == selectedId))
                .ToList();
        }
    }

    /// <summary>Loads the <see cref="StructureLookup"/> from the BL services (including inactive records, so every name can be resolved).</summary>
    public class StructureLookupService
    {
        private readonly IHotelService _hotels;
        private readonly IBlockService _blocks;
        private readonly IFloorService _floors;

        public StructureLookupService(IHotelService hotels, IBlockService blocks, IFloorService floors)
        {
            _hotels = hotels;
            _blocks = blocks;
            _floors = floors;
        }

        public async Task<StructureLookup> LoadAsync()
        {
            var hotels = await _hotels.GetAllAsync(includeInactive: true);
            var blocks = await _blocks.GetAllAsync(includeInactive: true);
            var floors = await _floors.GetAllAsync(includeInactive: true);
            return new StructureLookup(hotels, blocks, floors);
        }
    }
}
