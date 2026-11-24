using HotelApp.API.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    [Route("api/hotels")]
    public class HotelsController : ApiControllerBase
    {
        private readonly IHotelService _hotels;

        public HotelsController(IHotelService hotels)
        {
            _hotels = hotels;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<HotelDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var hotels = await _hotels.GetAllAsync(includeInactive);
            return Ok(hotels.Select(HotelDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<HotelDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelDto>> GetById(int id)
        {
            var hotel = await _hotels.GetByIdAsync(id);
            return hotel is null ? NotFoundProblem("El hotel no existe.") : Ok(HotelDto.FromEntity(hotel));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<HotelDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HotelDto>> Create(HotelCreateRequest request)
        {
            var hotel = request.ToEntity();
            await _hotels.AddAsync(hotel, CurrentUserId);

            return CreatedAtAction(nameof(GetById), new { id = hotel.HotelId }, HotelDto.FromEntity(hotel));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<HotelDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelDto>> Update(int id, HotelUpdateRequest request)
        {
            await _hotels.UpdateAsync(request.ToEntity(id, request.IsActive), CurrentUserId);

            var updated = await _hotels.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("El hotel no existe.") : Ok(HotelDto.FromEntity(updated));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _hotels.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}
