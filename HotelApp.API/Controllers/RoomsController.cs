using System.ComponentModel.DataAnnotations;
using HotelApp.API.Dtos;
using HotelApp.BL.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    /// <summary>
    /// Rooms. Any logged-in user can read, search availability (CU-004) and change the status (CU-012);
    /// creating, editing and taking rooms out of service is for the Administrator (CU-011 / RF-001).
    /// </summary>
    [Route("api/rooms")]
    public class RoomsController : ApiControllerBase
    {
        private readonly IRoomService _rooms;

        public RoomsController(IRoomService rooms)
        {
            _rooms = rooms;
        }

        /// <summary>Rooms ordered by block, floor and number; optionally filtered by hotel, block and/or floor. By default only the active ones.</summary>
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

        /// <summary>CU-004 / RF-004: free rooms for the dates and number of guests, with their price per night and estimated total.</summary>
        /// <param name="checkIn">Arrival date (yyyy-MM-dd).</param>
        /// <param name="checkOut">Departure date (yyyy-MM-dd), after the arrival date.</param>
        /// <param name="guests">Number of guests (at least 1).</param>
        /// <param name="hotelId">Optional: only rooms of this hotel.</param>
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

        /// <summary>Changes number, floor, type and whether the room is active. The status is changed with PATCH /api/rooms/{id}/status.</summary>
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

        /// <summary>CU-012 / RF-010: manual status change to "Disponible", "En Limpieza" or "Mantenimiento".</summary>
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

        /// <summary>RF-001 soft delete: the room is taken out of service. Not allowed while it has current reservations.</summary>
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
