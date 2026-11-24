using HotelApp.API.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    [Route("api/floors")]
    public class FloorsController : ApiControllerBase
    {
        private readonly IFloorService _floors;

        public FloorsController(IFloorService floors)
        {
            _floors = floors;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<FloorDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FloorDto>>> GetAll(
            [FromQuery] int? hotelId = null,
            [FromQuery] int? blockId = null,
            [FromQuery] bool includeInactive = false)
        {
            var floors = await _floors.GetAllAsync(hotelId, blockId, includeInactive);
            return Ok(floors.Select(FloorDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<FloorDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FloorDto>> GetById(int id)
        {
            var floor = await _floors.GetByIdAsync(id);
            return floor is null ? NotFoundProblem("El piso no existe.") : Ok(FloorDto.FromEntity(floor));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<FloorDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<FloorDto>> Create(FloorCreateRequest request)
        {
            var floor = request.ToEntity();
            await _floors.AddAsync(floor, CurrentUserId);

            return CreatedAtAction(nameof(GetById), new { id = floor.FloorId }, FloorDto.FromEntity(floor));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<FloorDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FloorDto>> Update(int id, FloorUpdateRequest request)
        {
            await _floors.UpdateAsync(request.ToEntity(id, request.IsActive), CurrentUserId);

            var updated = await _floors.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("El piso no existe.") : Ok(FloorDto.FromEntity(updated));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _floors.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}
