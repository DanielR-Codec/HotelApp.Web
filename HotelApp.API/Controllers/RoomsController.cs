using System.ComponentModel.DataAnnotations;
using HotelApp.API.Dtos;
using HotelApp.BL.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    [Route("api/rooms")]
    public class RoomsController : ApiControllerBase
    {
        private readonly IRoomService _rooms;

        public RoomsController(IRoomService rooms)
        {
            _rooms = rooms;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<RoomDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll(
            [FromQuery] int? hotelId = null,
            [FromQuery] int? blockId = null,
            [FromQuery] int? floorId = null,
            [FromQuery] bool includeInactive = false)
        {
            var rooms = await _rooms.GetAllAsync(hotelId, blockId, floorId, includeInactive);
            return Ok(rooms.Select(RoomDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<RoomDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDto>> GetById(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room is null ? NotFoundProblem("La habitación no existe.") : Ok(RoomDto.FromEntity(room));
        }

        [HttpGet("available")]
        [ProducesResponseType<IEnumerable<AvailableRoomDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AvailableRoomDto>>> SearchAvailability(
            [FromQuery, Required(ErrorMessage = "La fecha de llegada es obligatoria.")] DateTime? checkIn,
            [FromQuery, Required(ErrorMessage = "La fecha de salida es obligatoria.")] DateTime? checkOut,
            [FromQuery, Range(1, 20, ErrorMessage = "La cantidad de personas debe estar entre {1} y {2}.")] int guests = 1,
            [FromQuery] int? hotelId = null)
        {
            return Ok(await _rooms.SearchAvailabilityAsync(checkIn!.Value, checkOut!.Value, guests, hotelId));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<RoomDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoomDto>> Create(RoomCreateRequest request)
        {
            var room = request.ToEntity();
            await _rooms.AddAsync(room, CurrentUserId);

            return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, RoomDto.FromEntity(room));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<RoomDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDto>> Update(int id, RoomUpdateRequest request)
        {
            await _rooms.UpdateAsync(request.ToEntity(id, request.IsActive), CurrentUserId);

            var updated = await _rooms.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("La habitación no existe.") : Ok(RoomDto.FromEntity(updated));
        }

        [HttpPatch("{id:int}/status")]
        [ProducesResponseType<RoomDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDto>> ChangeStatus(int id, RoomStatusRequest request)
        {
            await _rooms.ChangeStatusAsync(id, request.Status, CurrentUserId);

            var updated = await _rooms.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("La habitación no existe.") : Ok(RoomDto.FromEntity(updated));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _rooms.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}
