using HotelApp.API.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    /// <summary>CU-010 / RF-002: room types and their base rate. Creating, changing rates or deleting is for the Administrator (RN-014).</summary>
    [Route("api/roomtypes")]
    public class RoomTypesController : ApiControllerBase
    {
        private readonly IRoomTypeService _roomTypes;

        public RoomTypesController(IRoomTypeService roomTypes)
        {
            _roomTypes = roomTypes;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<RoomTypeDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetAll()
        {
            var roomTypes = await _roomTypes.GetAllAsync();
            return Ok(roomTypes.Select(RoomTypeDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<RoomTypeDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomTypeDto>> GetById(int id)
        {
            var roomType = await _roomTypes.GetByIdAsync(id);
            return roomType is null ? NotFoundProblem("El tipo de habitación no existe.") : Ok(RoomTypeDto.FromEntity(roomType));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<RoomTypeDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoomTypeDto>> Create(RoomTypeRequest request)
        {
            var roomType = request.ToEntity();
            await _roomTypes.AddAsync(roomType, CurrentUserId);

            return CreatedAtAction(nameof(GetById), new { id = roomType.RoomTypeId }, RoomTypeDto.FromEntity(roomType));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<RoomTypeDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomTypeDto>> Update(int id, RoomTypeRequest request)
        {
            await _roomTypes.UpdateAsync(request.ToEntity(id), CurrentUserId);

            var updated = await _roomTypes.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("El tipo de habitación no existe.") : Ok(RoomTypeDto.FromEntity(updated));
        }

        /// <summary>Deletes the type. Not allowed while there are rooms that use it.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _roomTypes.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}
