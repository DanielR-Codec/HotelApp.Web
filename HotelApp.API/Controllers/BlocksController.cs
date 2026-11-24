using HotelApp.API.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    [Route("api/blocks")]
    public class BlocksController : ApiControllerBase
    {
        private readonly IBlockService _blocks;

        public BlocksController(IBlockService blocks)
        {
            _blocks = blocks;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<BlockDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BlockDto>>> GetAll([FromQuery] int? hotelId = null, [FromQuery] bool includeInactive = false)
        {
            var blocks = await _blocks.GetAllAsync(hotelId, includeInactive);
            return Ok(blocks.Select(BlockDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<BlockDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockDto>> GetById(int id)
        {
            var block = await _blocks.GetByIdAsync(id);
            return block is null ? NotFoundProblem("El bloque no existe.") : Ok(BlockDto.FromEntity(block));
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<BlockDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BlockDto>> Create(BlockCreateRequest request)
        {
            var block = request.ToEntity();
            await _blocks.AddAsync(block, CurrentUserId);

            return CreatedAtAction(nameof(GetById), new { id = block.BlockId }, BlockDto.FromEntity(block));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType<BlockDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockDto>> Update(int id, BlockUpdateRequest request)
        {
            await _blocks.UpdateAsync(request.ToEntity(id, request.IsActive), CurrentUserId);

            var updated = await _blocks.GetByIdAsync(id);
            return updated is null ? NotFoundProblem("El bloque no existe.") : Ok(BlockDto.FromEntity(updated));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRole.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _blocks.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}
